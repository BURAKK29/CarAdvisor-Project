using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.Domain.DTOs;

public class CarCompareDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Generation { get; set; } = string.Empty;
    public string BodyType { get; set; } = string.Empty;
    public string? Trim { get; set; }
}
