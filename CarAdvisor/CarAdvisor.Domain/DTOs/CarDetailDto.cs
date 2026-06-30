

namespace CarAdvisor.Domain.DTOs;

public class CarDetailDto
{
    public int Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Generation { get; set; }
    public string Trim { get; set; }
    public int Year { get; set; }
    public string BodyType { get; set; }
    public int Seats { get; set; }
    public int HorsePower { get; set; }
    public int EngineCC { get; set; }
    public double Acceleration { get; set; }
    public int MaxSpeed { get; set; }
    public string FuelType { get; set; }
    public string Transmission { get; set; }
    public double CityFuelConsumption { get; set; }
    public double AverageFuelConsumption { get; set; }

    // JOIN ile diğer tablodan (CarGenerations) gelecek veriler:
    public decimal? AveragePrice { get; set; }
    public string ImageUrl { get; set; }

}
