using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.DataSeeder;

public class CarCsvDto
{
    [Name("Make")]
    public string Make { get; set; }

    [Name("Modle")]
    public string Model { get; set; }

    [Name("Generation")]
    public string Generation { get; set; }

    [Name("Trim")]
    public string Trim { get; set; }

    [Name("Year_from")]
    public double? YearFrom { get; set; }

    [Name("Body_type")]
    public string BodyType { get; set; }

    [Name("number_of_seats")]
    public string Seats { get; set; }

    // --- DÜZELTME: int? -> double? yapıldı ---
    // CSV'de "4500.0" gelirse hata vermemesi için double yapıyoruz.
    // Ayrıca "lenght" -> "length" yazım hatası düzeltildi.

    [Name("length_mm")]
    public double? Length { get; set; }

    [Name("width_mm")]
    public double? Width { get; set; }

    [Name("height_mm")]
    public double? Height { get; set; }

    // --- DÜZELTME: Eksik olan Name etiketi eklendi ve double yapıldı ---
    [Name("max_trunk_capacity_l")]
    public double? TrunkCapacity { get; set; }

    // motor performans
    [Name("engine_hp")]
    public double? HorsePower { get; set; }

    [Name("capacity_cm3")]
    public double? EngineCC { get; set; }

    [Name("max_speed_km_per_h")]
    public string? MaxSpeed { get; set; }

    [Name("acceleration_0_100_km/h_s")]
    public double? Acceleration { get; set; }

    [Name("transmission")]
    public string Transmission { get; set; }

    [Name("engine_type")]
    public string EngineType { get; set; }

    // yakıt tüketimi
    [Name("city_fuel_per_100km_l")]
    public double? CityConsumption { get; set; }

    [Name("highway_fuel_per_100km_l")]
    public double? HighwayConsumption { get; set; }

    [Name("mixed_fuel_consumption_per_100_km_l")]
    public double? MixedConsumption { get; set; }
}