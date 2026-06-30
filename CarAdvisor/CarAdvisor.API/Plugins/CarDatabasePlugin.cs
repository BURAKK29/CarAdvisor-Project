using CarAdvisor.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace CarAdvisor.API.Plugins
{
    public class CarDatabasePlugin
    {
        private readonly CarAdvisorContext _context;

        public CarDatabasePlugin(CarAdvisorContext context)
        {
            _context = context;
        }

        [KernelFunction("filter_cars")]
        [Description("""
            Kullanıcının doğal dil araç talebini analiz ederek veritabanından en uygun araçları filtreler ve listeler.
            Doğal dil → parametre eşlemeleri:
            - "Aile arabası / geniş / 4-5 kişilik" → bodyType: SUV veya MPV
            - "Az yakan / ekonomik / yakıt tasarruflu" → fuelType: Dizel veya Hibrit, maxFuelConsumption: 7.0
            - "Sportif / güçlü / performanslı" → minHorsePower: 200
            - "Şehir içi / küçük / kompakt" → bodyType: Hatchback
            - "Konforlu / rahat / lüks" → premium markalar veya bodyType: Sedan/SUV
            - "Yeni model / güncel" → minYear: 2020
            - "Uygun fiyatlı / ucuz" → maxPrice düşük tut
            - "Manuel istemiyorum" → transmission: Otomatik
            """)]
        public async Task<string> FilterCarsAsync(
            [Description("Yakıt tipi. Değerler: 'Benzin', 'Dizel', 'Elektrik', 'Hibrit'. 'Az yakan' veya 'ekonomik' → Dizel veya Hibrit. Belirtilmemişse null.")]
            string? fuelType = null,

            [Description("Vites tipi. 'Otomatik' veya 'Manuel'. Belirtilmemişse null.")]
            string? transmission = null,

            [Description("Kasa tipi. 'Sedan', 'SUV', 'Hatchback', 'Coupe', 'MPV', 'Station Wagon'. Aile için SUV/MPV, şehir için Hatchback, spor için Coupe. Emin değilsen null bırak.")]
            string? bodyType = null,

            [Description("Maksimum bütçe TL cinsinden (tam sayı). '1.5 milyon TL' = 1500000, '800 bin' = 800000. Belirtilmemişse null.")]
            int? maxPrice = null,

            [Description("Minimum bütçe TL cinsinden. Belirtilmemişse null.")]
            int? minPrice = null,

            [Description("Minimum model yılı. 'Yeni model / güncel' → 2020, 'Son çıkan' → 2022. Belirtilmemişse null.")]
            int? minYear = null,

            [Description("Maksimum model yılı. Belirtilmemişse null.")]
            int? maxYear = null,

            [Description("Kullanıcının açıkça istediği markalar listesi. Örn: ['BMW', 'Audi']. Sadece açıkça belirtilmişse doldur.")]
            List<string>? brands = null,

            [Description("Kullanıcının istemediği markalar listesi. 'X markası olmasın' diyorsa buraya yaz.")]
            List<string>? excludeBrands = null,

            [Description("Maksimum yakıt tüketimi L/100km. 'Az yakan', 'ekonomik', 'yakıt tasarruflu' için 7.0 kullan. Belirtilmemişse null.")]
            double? maxFuelConsumption = null,

            [Description("Minimum beygir gücü (HP). 'Güçlü', 'sportif', 'performanslı' için 180 kullan. Belirtilmemişse null.")]
            int? minHorsePower = null)
        {
            try
            {
                var carQuery = _context.Cars.AsQueryable();

                if (brands != null && brands.Any())
                {
                    var included = brands.Select(b => b.ToLower()).ToList();
                    carQuery = carQuery.Where(c => included.Contains(c.Make.ToLower()));
                }

                if (!string.IsNullOrEmpty(fuelType))
                    carQuery = carQuery.Where(c => c.FuelType.ToLower().Contains(fuelType.ToLower()));

                if (!string.IsNullOrEmpty(transmission))
                    carQuery = carQuery.Where(c => c.Transmission.ToLower().Contains(transmission.ToLower()));

                if (!string.IsNullOrEmpty(bodyType))
                    carQuery = carQuery.Where(c => c.BodyType.ToLower().Contains(bodyType.ToLower()));

                if (minYear.HasValue)
                    carQuery = carQuery.Where(c => c.Year >= minYear.Value);

                if (maxYear.HasValue)
                    carQuery = carQuery.Where(c => c.Year <= maxYear.Value);

                if (excludeBrands != null && excludeBrands.Any())
                {
                    var excluded = excludeBrands.Select(b => b.ToLower()).ToList();
                    carQuery = carQuery.Where(c => !excluded.Contains(c.Make.ToLower()));
                }

                if (minHorsePower.HasValue)
                    carQuery = carQuery.Where(c => c.HorsePower >= minHorsePower.Value);

                if (maxFuelConsumption.HasValue)
                {
                    var maxFuel = maxFuelConsumption.Value;
                    // Verisi olmayan araçları (0) dahil et; sadece verisi olan ve limiti aşanları dışla
                    carQuery = carQuery.Where(c => c.AverageFuelConsumption == 0 || c.AverageFuelConsumption <= maxFuel);
                }

                var cars = await carQuery
                    .OrderByDescending(c => c.Year)
                    .ToListAsync();

                var makes = cars.Select(c => c.Make).Distinct().ToList();
                var models = cars.Select(c => c.Model).Distinct().ToList();

                var generations = await _context.CarGenerations
                    .Where(g => makes.Contains(g.Brand) && models.Contains(g.Model) && g.AveragePrice > 0)
                    .ToListAsync();

                var joined = cars
                    .Select(car =>
                    {
                        var gen = generations.FirstOrDefault(g =>
                            g.Brand == car.Make &&
                            g.Model == car.Model &&
                            g.GenerationName == car.Generation &&
                            g.BodyType == car.BodyType);

                        return new
                        {
                            car.Make,
                            car.Model,
                            car.Generation,
                            car.Year,
                            car.BodyType,
                            car.FuelType,
                            car.Transmission,
                            car.AverageFuelConsumption,
                            car.HorsePower,
                            AveragePrice = gen?.AveragePrice ?? 0,
                            ImageUrl = gen?.ImageUrl ?? "NotFound"
                        };
                    })
                    .Where(c => c.AveragePrice > 0)
                    .ToList();

                if (minPrice.HasValue)
                    joined = joined.Where(c => c.AveragePrice >= minPrice.Value).ToList();

                if (maxPrice.HasValue)
                    joined = joined.Where(c => c.AveragePrice <= maxPrice.Value).ToList();

                if (!joined.Any())
                    return "Kriterlere uygun araç bulunamadı. Kullanıcıya farklı kriterler denemesini öner.";

                var distinct = joined
                    .GroupBy(c => new { c.Make, c.Model })
                    .Select(g => g.OrderByDescending(c => c.AveragePrice).First())
                    .OrderByDescending(c => c.AveragePrice)
                    .Take(6)
                    .ToList();

                var result = "İşte size uygun araçlar:\n\n";

                foreach (var car in distinct)
                {
                    string safeMake = Uri.EscapeDataString(car.Make ?? "");
                    string safeModel = Uri.EscapeDataString(car.Model ?? "");
                    string safeGeneration = Uri.EscapeDataString(car.Generation ?? "");
                    string safeBodyType = Uri.EscapeDataString(car.BodyType ?? "");

                    result += $"> ![{car.Make} {car.Model}]({car.ImageUrl})\n";
                    result += $"> ### {car.Make} {car.Model} ({car.Generation})\n";
                    result += $"> - Yıl: {car.Year}\n";
                    result += $"> - Kasa: {car.BodyType}\n";
                    result += $"> - Yakıt: {car.FuelType}\n";
                    result += $"> - Vites: {car.Transmission}\n";
                    result += $"> - Performans: {car.HorsePower} HP - {car.AverageFuelConsumption:F1} L/100km\n";
                    result += $"> - Fiyat: 💰 {car.AveragePrice:N0} ₺\n";
                    result += $">\n";
                    result += $"> [Aracı İncele](/details/{safeMake}/{safeModel}/{safeGeneration}/{safeBodyType})\n\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Veritabanı hatası: {ex.Message}";
            }
        }
    }
}