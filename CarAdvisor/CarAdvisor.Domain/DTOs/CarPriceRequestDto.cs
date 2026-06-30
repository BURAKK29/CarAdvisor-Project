using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.Domain.DTOs
{
    public class CarPriceRequestDto
    {
       
            public int CarId { get; set; }
            public string Make { get; set; }
            public string Model { get; set; }
            public int StartYear { get; set; }
            public int EndYear { get; set; }
        public string BodyType { get; set; }


    }
}
