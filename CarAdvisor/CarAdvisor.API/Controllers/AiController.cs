using CarAdvisor.API.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace CarAdvisor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly CarDatabasePlugin _carPlugin;
        private static readonly ConcurrentDictionary<string, ChatHistory> _sessions = new();

        private const string ASSISTANT_SYSTEM_PROMPT =
            "Sen CarAdvisor'ın samimi ve uzman yapay zeka araç danışmanısın. " +
            "Veritabanından gelen araç listesini kullanıcıya sıcak, doğal ve profesyonel bir dille sun. " +
            "AŞIRI ÖNEMLİ: Veritabanından gelen '>' (blockquote) ile başlayan yapıları, " +
            "içindeki liste elemanlarını ve linkleri ASLA değiştirme, olduğu gibi koru. " +
            "Kısa bir giriş yaz, ardından araçları listele. Asla hayali araç uydurma.";

        public AiController(Kernel kernel, CarDatabasePlugin carPlugin)
        {
            _kernel = kernel;
            _chatService = kernel.GetRequiredService<IChatCompletionService>();
            _carPlugin = carPlugin;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAi([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserQuestion))
                return BadRequest(new { answer = "Soru boş olamaz." });

            var sessionId = request.SessionId ?? "default";

            // Akıllı doğal dil extraction prompt'u:
            // Hem "dizel, sedan, otomatik" hem de "4 kişilik aile, az yakan" gibi sorguları yorumlar
            string extractPrompt = $@"
Aşağıdaki kullanıcı talebini analiz et ve araç filtresi JSON nesnesine dönüştür.

DOĞAL DİL → PARAMETRE EŞLEMELERİ:
- 'aile arabası', 'geniş', '4-5 kişilik', 'çocuklu aile' → bodyType: 'SUV' veya 'MPV'
- 'az yakan', 'ekonomik', 'yakıt tasarruflu', 'masrafsız' → SADECE fuelType: 'Dizel' (maxFuelConsumption KULLANMA)
- 'sportif', 'güçlü', 'performanslı', 'hızlı' → minHorsePower: 180
- 'şehir içi', 'küçük', 'kompakt', 'dar sokak' → bodyType: 'Hatchback'
- 'konforlu', 'rahat', 'lüks' → bodyType: 'Sedan' veya 'SUV'
- 'yeni model', 'güncel', 'son model' → minYear: 2022
- 'eski olabilir', 'ikinci el uygun' → minYear null bırak
- Bütçe belirtilmişse maxPrice'a yaz:
  '1 milyon' = 1000000
  '1.5 milyon' = 1500000
  '500 bin' = 500000
  '800 bin' = 800000
  '2 milyon' = 2000000

KURULLAR:
- maxFuelConsumption: her zaman null bırak (yakıt tüketimi filtresi fuelType ile yapılır)
- Eğer bir özellik belirtilmemişse null yaz
- Marka açıkça söylenmemişse brands null bırak
- Sadece JSON döndür, başka kelime yazma

JSON FORMAT:
{{
  ""fuelType"": ""Benzin/Dizel/Elektrik/Hibrit veya null"",
  ""transmission"": ""Otomatik/Manuel veya null"",
  ""bodyType"": ""Sedan/Hatchback/SUV/MPV/Coupe/Station Wagon veya null"",
  ""maxPrice"": sayı veya null,
  ""minPrice"": sayı veya null,
  ""minYear"": sayı veya null,
  ""maxYear"": sayı veya null,
  ""brands"": ""virgülle ayrılmış markalar veya null"",
  ""excludeBrands"": ""virgülle ayrılmış markalar veya null"",
  ""maxFuelConsumption"": null,
  ""minHorsePower"": sayı (HP) veya null
}}

Kullanıcı talebi: {request.UserQuestion}
";

            try
            {
                // Adım 1: Doğal dili filtre parametrelerine çevir
                var extractionChat = new ChatHistory("Sen bir araç filtresi JSON çıkartıcısısın. Sadece JSON döndür.");
                extractionChat.AddUserMessage(extractPrompt);

                ChatMessageContent extractionResult = null;
                int retryCount = 0;
                while (retryCount < 3)
                {
                    try
                    {
                        extractionResult = await _chatService.GetChatMessageContentAsync(extractionChat);
                        break;
                    }
                    catch (Exception ex) when (ex.Message.Contains("503") || ex.Message.Contains("Service Unavailable"))
                    {
                        retryCount++;
                        await Task.Delay(1500 * retryCount);
                    }
                }

                if (extractionResult == null)
                    throw new Exception("Google AI servisi şu an çok yoğun. Lütfen tekrar deneyin.");

                // JSON parse
                string jsonText = extractionResult.Content ?? "{}";
                var match = Regex.Match(jsonText, @"\{[\s\S]*\}");
                string cleanJson = match.Success ? match.Value : "{}";

                var filter = JsonSerializer.Deserialize<CarFilterParams>(cleanJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CarFilterParams();

                var brandList = string.IsNullOrWhiteSpace(filter.Brands)
                    ? null : filter.Brands.Split(',').Select(x => x.Trim()).ToList();
                var excludeBrandList = string.IsNullOrWhiteSpace(filter.ExcludeBrands)
                    ? null : filter.ExcludeBrands.Split(',').Select(x => x.Trim()).ToList();

                // Adım 2: Veritabanından araçları getir — bulunamazsa kademeli fallback
                string dbResult = await _carPlugin.FilterCarsAsync(
                    filter.FuelType, filter.Transmission, filter.BodyType,
                    filter.MaxPrice, filter.MinPrice,
                    filter.MinYear, filter.MaxYear,
                    brandList, excludeBrandList,
                    null, filter.MinHorsePower  // maxFuelConsumption her zaman null
                );

                // Fallback 1: Sonuç yok → bodyType ve transmission kaldır
                if (dbResult.Contains("bulunamad"))
                {
                    dbResult = await _carPlugin.FilterCarsAsync(
                        filter.FuelType, null, null,
                        filter.MaxPrice, filter.MinPrice,
                        filter.MinYear, filter.MaxYear,
                        brandList, null,
                        null, null
                    );
                }

                // Fallback 2: Hâlâ sonuç yok → yıl filtresi de kaldır
                if (dbResult.Contains("bulunamad"))
                {
                    dbResult = await _carPlugin.FilterCarsAsync(
                        filter.FuelType, null, null,
                        filter.MaxPrice, filter.MinPrice,
                        null, null,
                        null, null,
                        null, null
                    );
                }

                // Fallback 3: Hâlâ sonuç yok → sadece fiyat aralığı
                if (dbResult.Contains("bulunamad"))
                {
                    dbResult = await _carPlugin.FilterCarsAsync(
                        null, null, null,
                        filter.MaxPrice, filter.MinPrice,
                        null, null,
                        null, null,
                        null, null
                    );
                }

                // Adım 3: Sonucu doğal dille sun (oturum hafızalı)
                var history = _sessions.GetOrAdd(sessionId, _ => new ChatHistory(ASSISTANT_SYSTEM_PROMPT));

                history.AddUserMessage(request.UserQuestion);
                history.AddSystemMessage("Veritabanı Sonucu:\n" + dbResult);

                ChatMessageContent finalResponse = null;
                retryCount = 0;
                while (retryCount < 3)
                {
                    try
                    {
                        finalResponse = await _chatService.GetChatMessageContentAsync(history);
                        break;
                    }
                    catch (Exception ex) when (ex.Message.Contains("503") || ex.Message.Contains("Service Unavailable"))
                    {
                        retryCount++;
                        await Task.Delay(1500 * retryCount);
                    }
                }

                if (finalResponse == null)
                    throw new Exception("Google AI servisi şu an çok yoğun. Lütfen tekrar deneyin.");

                string answer = finalResponse.Content ?? "Şu anda uygun araç bulamıyorum.";

                // Geçici system mesajını temizle, history'ye assistant yanıtını ekle
                var tempSys = history.LastOrDefault(m =>
                    m.Role == AuthorRole.System && (m.Content?.StartsWith("Veritabanı Sonucu") ?? false));
                if (tempSys != null) history.Remove(tempSys);

                history.AddAssistantMessage(answer);

                // Hafızayı koru (ilk sistem + son 6 mesaj)
                if (history.Count > 14)
                {
                    var sys = history[0];
                    var recent = history.TakeLast(6).ToList();
                    history.Clear();
                    history.Add(sys);
                    history.AddRange(recent);
                }

                return Ok(new { answer });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Hata: {ex.Message}");
                return Ok(new { answer = $"Sistem şu an öneri oluşturamıyor, lütfen tekrar deneyin. ({ex.Message})" });
            }
        }

        [HttpDelete("session/{sessionId}")]
        public IActionResult ClearSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
            return Ok(new { message = "Oturum temizlendi." });
        }

        [HttpPost("compare-expert")]
        public async Task<IActionResult> CompareExpert([FromBody] JsonElement carsData)
        {
            string carsJson = carsData.GetRawText();
            string prompt = $@"
Sen üst düzey bir Otomobil Baş Uzmanısın! Aşağıda kıyaslanan araçların ham teknik verilerini (JSON) incele. 
Bana KESİNLİKLE sadece şu formatta geçerli ve eksiksiz bir JSON dön. Asla Markdown (backticks vs.) ile başlama, sadece JSON:
{{
  ""expertVerdict"": ""Araçların genel kıyaslamasını, kimin neye göre seçmesi gerektiğini ve kazananı detaylı anlatan Şık ve Profesyonel, bold ve listeler kullanan bir Markdown metni."",
  ""winnerCarFullName"": ""Tam Araç Markası ve Modeli (Eğer net bir kazanan varsa, örn: 'BMW 1 Series' veya 'Audi A1'). Yoksa boş bırak."",
  ""carEvaluations"": {{
      ""Tam Araç Markası ve Modeli (Örn: Renault Clio)"": {{ ""handling"": ""İyi/Orta/Kötü"", ""comfort"": ""İyi/Orta/Kötü"", ""safety"": ""İyi/Orta/Kötü"" }}
  }}
}}

Araç Verileri:
{carsJson}
";
            try
            {
                var settings = new GeminiPromptExecutionSettings { Temperature = 0.4 };
                var response = await _chatService.GetChatMessageContentAsync(prompt, settings, _kernel);
                string responseText = response.Content ?? "{}";

                var match = Regex.Match(responseText, @"\{[\s\S]*\}");
                string cleanJson = match.Success ? match.Value : "{}";

                return Content(cleanJson, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class AskRequest
    {
        public string UserQuestion { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }

    public class CarFilterParams
    {
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? BodyType { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinPrice { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public string? Brands { get; set; }
        public string? ExcludeBrands { get; set; }
        public double? MaxFuelConsumption { get; set; }
        public int? MinHorsePower { get; set; }
    }
}
