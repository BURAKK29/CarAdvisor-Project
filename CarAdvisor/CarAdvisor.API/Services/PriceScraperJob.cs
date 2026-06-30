using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Hangfire;

public class PriceScraperJob
{
    private readonly IConfiguration _configuration;

    public PriceScraperJob(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AutomaticRetry(Attempts = 4)]
    public async Task ScrapePriceAsync(int carId, string make, string model, int startYear, int endYear, string bodyType)
    {
        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
            });
            var page = await context.NewPageAsync();

            // 1. Marka ve Modeli küçük harfe çevir
            string safeMake = make.ToLower().Trim();
            string safeModel = model.ToLower().Trim();

            // 2. İngilizce "series" kelimesini sitenin anladığı "serisi" kelimesine çevir (BMW 1 Series -> bmw 1 serisi)
            safeModel = safeModel.Replace("series", "serisi");

            // YENİ: Audi RS modellerindeki "rs-rs-" tekrarını düzelt (Örn: rs-rs-6 -> rs-6)
            safeModel = safeModel.Replace("rs-rs-", "rs-");

            // 3. Aradaki tüm boşlukları tire (-) işareti ile değiştir (bmw 1 serisi -> bmw-1-serisi)
            safeMake = safeMake.Replace(" ", "-");
            safeModel = safeModel.Replace(" ", "-");

            // --- KATEGORİ SEÇİMİ ---
            string category = "otomobil";

            if (!string.IsNullOrEmpty(bodyType))
            {
                // Eğer araç SUV ise
                if (bodyType.Equals("SUV", StringComparison.OrdinalIgnoreCase))
                {
                    category = "arazi-suv-pick-up";
                }
                // EĞER ARAÇ TİCARİ (Minivan/Panelvan) İSE YENİ KATEGORİ:
                else if (bodyType.Equals("Minivan", StringComparison.OrdinalIgnoreCase) ||
                         bodyType.Equals("Panelvan", StringComparison.OrdinalIgnoreCase))
                {
                    category = "minivan-panelvan";
                }
            }

            // 4. Tertemiz ve kategoriye özel URL'yi oluştur
            string targetUrl = $"https://www.arabam.com/ikinci-el/{category}/{safeMake}-{safeModel}?minYear={startYear}&maxYear={endYear}";

            Console.WriteLine($"[Playwright] Hedef siteye gidiliyor: {targetUrl}");
            await page.GotoAsync(targetUrl, new PageGotoOptions
            {
                Timeout = 50000, // Sabrımızı 30 saniyeden 60 saniyeye çıkarıyoruz
                WaitUntil = WaitUntilState.DOMContentLoaded // Sitedeki tüm resimlerin yüklenmesini bekleme, sadece metinler (fiyatlar) gelince duruma "Tamam" de!
            });
            // Timeout patlamalarını önlemek için sadece HTML'i bekle
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.WaitForTimeoutAsync(3000);

            // --- YENİ EKLENEN KISIM: ÇEREZ (COOKIE) TIKLAYICI ---
            try
            {
                // Sayfada "Kabul Et" butonunu ara
                var cookieButton = page.Locator("button:has-text('Kabul Et'), a:has-text('Kabul Et')").First;
                if (await cookieButton.IsVisibleAsync())
                {
                    Console.WriteLine("[Playwright] Çerez penceresi tespit edildi, 'Kabul Et' butonuna tıklanıyor...");
                    await cookieButton.ClickAsync();
                    await page.WaitForTimeoutAsync(1500); // Pencerenin kapanması için kısa bir süre bekle
                }
            }
            catch
            {
                // Buton yoksa veya tıklanmazsa sessizce devam et, sistemi durdurma.
            }

            // 1. KONTROL: SİTE BİZİ GERÇEKTEN ENGELLEDİ Mİ? (Akıllandırılmış Bot Tespiti)
            string pageTitle = await page.TitleAsync();
            string pageText = await page.InnerTextAsync("body");
            string pageTextLower = pageText.ToLower();

            // Sadece gerçek Cloudflare/CAPTCHA duvarlarında hata fırlatması için kelimeleri netleştirdik
            if (pageTitle.Contains("Just a moment") || pageTextLower.Contains("robot musunuz") || pageTextLower.Contains("cloudflare"))
            {
                string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"BAN_YEDIK_{carId}.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = debugPath });

                throw new Exception($"[ENGEL TESPİT EDİLDİ] Site güvenlik duvarı bizi durdurdu! Ekran görüntüsü alındı: {debugPath}");
            }

            // 2. ADIM: FİYATLARI ÇEKME
            var priceElements = await page.Locator(".listing-price, .price").AllInnerTextsAsync();
            List<decimal> foundPrices = new List<decimal>();

            foreach (var priceText in priceElements)
            {
                string cleanText = Regex.Replace(priceText, @"[^\d]", "");
                if (decimal.TryParse(cleanText, out decimal price) && price > 100000)
                {
                    foundPrices.Add(price);
                }
            }

            decimal finalAveragePrice = 0;

            // 3. KONTROL: İLAN YOK MU?
            if (!foundPrices.Any())
            {
                // HATA FIRLATMIYORUZ! Sadece log yazıp geçiyoruz.
                Console.WriteLine($"[Bilgi] {startYear} {endYear} {make} {model} için sitede hiç ilan YOK. Veritabanına 0 yazılarak es geçiliyor...");
                finalAveragePrice = 0;
            }
            else
            {
                finalAveragePrice = foundPrices.Average();
                Console.WriteLine($"[Playwright] BAŞARILI! {foundPrices.Count} ilan bulundu. Ortalama: {finalAveragePrice:N0} TL");
            }

            // 4. ADIM: DAPPER İLE SQL'İ GÜNCELLE
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                // finalAveragePrice eğer ilan yoksa 0 olarak veritabanına yazılacak.
                // n8n artık "AveragePrice IS NULL" sorgusunda bunu bulamayacağı için tekrar tekrar sormayacak!
                string sqlQuery = @"UPDATE CarGenerations SET AveragePrice = @Price, PriceLastUpdated = GETDATE() WHERE Id = @Id";
                await connection.ExecuteAsync(sqlQuery, new { Price = finalAveragePrice, Id = carId });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Hata] Scraper başarısız: {ex.Message}");
            throw;
        }
    }
}