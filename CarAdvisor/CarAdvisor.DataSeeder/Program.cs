using CarAdvisor.DataSeeder;
using CarAdvisor.Domain.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore; // DbContext için gerekli
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// (SeederDbContext sınıfı aynen kalıyor - EF Core referanslarını eklemeyi unutma)
public class SeederDbContext : DbContext
{
    public DbSet<Car> Cars { get; set; } // DbSet: Veritabanındaki tablonun ismini temsil eder.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=DESKTOP-I5L5LLB;Database=CarAdvisor;Trusted_Connection=True;TrustServerCertificate=True;",
            o => o.EnableRetryOnFailure()
        );
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== GÜNCEL VERİ SETİ YÜKLEME ===");
        string dosyaYolu = "Car_DataSet.csv"; // Dosya isminizi kontrol edin

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",// Sütunların virgülle ayrıldığını belirtiyoruz.
            MissingFieldFound = null,// "Eğer DTO'da yazdığım bir sütun CSV'de yoksa, hata verme, geç" demek.
            HeaderValidated = null,
            BadDataFound = null
        };

        var eklenecekArabalar = new List<Car>();

        using (var reader = new StreamReader(dosyaYolu)) //StreamReader: Dosyayı fiziksel olarak açar ve karakter akışını başlatır.
        using (var csv = new CsvReader(reader, config))
        {
            var records = csv.GetRecords<CarCsvDto>();
            // GetRecords: Tüm satırları tek seferde okur ve hafızaya 'CarCsvDto' listesi olarak yükler.

            foreach (var r in records)
            {
                // --- 1. YAKIT TİPİ DÖNÜŞÜMÜ (engine_type sütununa göre) ---
                string yakit = "Benzin"; // Varsayılan
                if (!string.IsNullOrEmpty(r.EngineType))
                {
                    string et = r.EngineType.ToLower();
                    if (et.Contains("diesel")) yakit = "Dizel";
                    else if (et.Contains("gasoline")) yakit = "Benzin";
                    else if (et.Contains("hybrid")) yakit = "Hibrit";
                    else if (et.Contains("electric")) yakit = "Elektrik";
                }

                // --- 2. VİTES TİPİ DÖNÜŞÜMÜ (transmission sütununa göre) ---
                // Resimde "robot" ve "CVT" gördüm, bunları da Otomatik sayıyoruz.
                string vites = "Manuel";
                if (!string.IsNullOrEmpty(r.Transmission))
                {
                    string tr = r.Transmission.ToLower();
                    if (tr.Contains("automatic") || tr.Contains("robot") || tr.Contains("variable") || tr.Contains("cvt"))
                    {
                        vites = "Otomatik";
                    }
                }

                // 3. Max Speed Temizliği ("220 km/h" -> 220)
                int sonHiz = 0;
                if (!string.IsNullOrEmpty(r.MaxSpeed))
                {
                    // "km/h" yazısını sil, boşlukları temizle
                    string temizHiz = r.MaxSpeed.ToLower().Replace("km/h", "").Trim();

                    // Sayıya çevirmeyi dene
                    if (double.TryParse(temizHiz, NumberStyles.Any, CultureInfo.InvariantCulture, out double h))
                    {
                        sonHiz = (int)h;
                    }
                }
                // --- 4. SEATS (KOLTUK) TEMİZLİĞİ ---
                // Sorun: "4, 5" gibi gelen verileri temizliyoruz.
                int koltukSayisi = 0;
                if (!string.IsNullOrEmpty(r.Seats))
                {
                    // Virgülden böl ve ilkini al (Örn: "4, 5" -> "4")
                    string temizKoltuk = r.Seats.Split(',')[0].Trim();

                    if (double.TryParse(temizKoltuk, NumberStyles.Any, CultureInfo.InvariantCulture, out double s))
                    {


                        koltukSayisi = (int)s;
                    }
                }

                // Nesne Başlatıcı (Object Initializer): MAPPING YAPIYOR.
                var car = new Car
                {
                    Make = r.Make,
                    Model = r.Model,
                    Generation = r.Generation,
                    Trim = r.Trim,
                    Year = (int)(r.YearFrom ?? 0),//null ise 0 ata
                    BodyType = r.BodyType,
                    Seats = koltukSayisi,

                    HorsePower = (int)(r.HorsePower ?? 0),
                    EngineCC = (int)(r.EngineCC ?? 0),
                    MaxSpeed = sonHiz,
                    Acceleration = r.Acceleration ?? 0,

                    // Artık tahmin değil, net veri:
                    FuelType = yakit,
                    Transmission = vites,

                    CityFuelConsumption = r.CityConsumption ?? 0,
                    HighwayFuelConsumption = r.HighwayConsumption ?? 0,
                    AverageFuelConsumption = r.MixedConsumption ?? 0,

                    // --- DÜZELTME: DTO'da double? yaptık, burada int'e çeviriyoruz ---
                    Length = (int)(r.Length ?? 0),
                    Width = (int)(r.Width ?? 0),
                    Height = (int)(r.Height ?? 0),
                    TrunkCapacity = (int)(r.TrunkCapacity ?? 0)
                };
               

                eklenecekArabalar.Add(car);
            }
        }

        Console.WriteLine($"{eklenecekArabalar.Count} araç başarıyla işlendi.");

        // Veritabanına kaydetme kısmı
        using (var context = new SeederDbContext())
        {
            context.Database.EnsureCreated();

            if (!context.Cars.Any())
            {
                Console.WriteLine("Veritabanına yazılıyor...");
                context.Cars.AddRange(eklenecekArabalar);
                context.SaveChanges();
                Console.WriteLine("✅ İŞLEM BAŞARILI!");
            }
            else
            {
                Console.WriteLine("⚠️ Veritabanı zaten dolu, ekleme yapılmadı.");
            }
        }

        Console.ReadKey();
    }
}