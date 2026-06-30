using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.Domain.Entities;

public class Car
{
    [Key]
    public int Id { get; set; } // SQL'deki Primary Key

    // --- Kimlik Bilgileri ---
    public string Make { get; set; }        // Marka
    public string Model { get; set; }       // Model
    public string Generation { get; set; }  // Jenerasyon
    public string Trim { get; set; }        // Paket
    public int Year { get; set; }           // Yıl

    // --- Kategorizasyon ---
    public string BodyType { get; set; }    // Kasa Tipi
    public int Seats { get; set; }          // Koltuk Sayısı

    // --- Teknik ---
    public int HorsePower { get; set; }
    public int EngineCC { get; set; }
    public double Acceleration { get; set; } // 0-100
    public int MaxSpeed { get; set; }

    // --- Yakıt & Vites (Temizlenmiş Veri) ---
    public string FuelType { get; set; }    // Benzin, Dizel...
    public string Transmission { get; set; } // Otomatik, Manuel...

    // --- Tüketim ---
    public double CityFuelConsumption { get; set; }
    public double HighwayFuelConsumption { get; set; }
    public double AverageFuelConsumption { get; set; }

    // --- Ebatlar ---
    //public int? AveragePrice { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int TrunkCapacity { get; set; } // Bagaj
}
