using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.Domain.DTOs;

public class CarListDto
{
    public int? Id { get; set; }

    // Marka ve Modeli ayrı ayrı değil, birleşik gönderebiliriz.
    // Örn: "Renault Megane"                        
    public string? BrandModel { get; set; }

    public string? Package { get; set; }     // Trim (Paket)
    public int? Year { get; set; }
    public string? BodyType { get; set; }
    public string? Fuel { get; set; }        // FuelType
    public string? Gear { get; set; }        // Transmission
    public int? HorsePower { get; set; }

    // Ekranda göstermek için tüketim (Ortalama)
    public double? Consumption { get; set; }
}
