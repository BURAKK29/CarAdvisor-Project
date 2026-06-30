using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.Domain.Entities;

public class CarGeneration
{
    public int Id { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? GenerationName { get; set; }
    public string? BodyType { get; set; } // Yeni eklediğimiz kritik alan
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public string? ImageUrl { get; set; }
    public int? AveragePrice { get; set; }
}
