using CarAdvisor.Business.Abstract;
using CarAdvisor.Domain.DTOs;
using CarAdvisor.DataAccess.Contexts;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CarAdvisor.Domain.Entities;

namespace CarAdvisor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        public readonly ICarManager _carManager;
        private readonly ProducerConfig _kafkaConfig;
        private readonly CarAdvisorContext _context;

        public CarsController(ICarManager carManager, CarAdvisorContext context)
        {
            _carManager = carManager;
            _context = context;

            // Kafka'nÄ±n adresini sisteme tanÄ±tÄ±yoruz
            _kafkaConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var cars = _carManager.GetAll();
            if (cars != null)
            {
                return Ok(cars);
            }
            return BadRequest("Veri bulunamadÄ±...");
        }

        [HttpGet("getbrands")]
        public IActionResult GetBrands()
        {
            var brands = _carManager.GetBrands();
            if (brands != null)
            {
                return Ok(brands);
            }
            return BadRequest("Marka bilgisi bulunamadÄ±...");
        }

        [HttpGet("models/{brand}")]
        public IActionResult GetGenerationsByBrand(string brand)
        {
            var models = _carManager.GetGenerationsByBrand(brand);
            if (models != null)
            {
                return Ok(models);
            }
            return BadRequest("Model bilgisi bulunamadÄ±...");
        }

        [HttpGet("getcardetails")]
        public IActionResult GetCarDetails([FromQuery] string brand, [FromQuery] string model, [FromQuery] string generation, [FromQuery] string bodyType)
        {
            brand = brand ?? "";
            model = model ?? "";
            generation = generation ?? "";
            bodyType = bodyType ?? "";

            var carDetails = _carManager.GetCarDetails(brand, model, generation, bodyType);

            if (carDetails != null && carDetails.Count > 0)
            {
                return Ok(carDetails);
            }
            return NotFound("Bu araca ait detay bulunamadÄ±.");
        }

        [HttpGet("getbybodytype")]
        public IActionResult GetByBodyType([FromQuery] string bodyType)
        {
            var result = _carManager.GetGenerationsByBodyType(bodyType);
            if (result != null && result.Count > 0)
            {
                return Ok(result);
            }
            return NotFound("Bu kasa tipine ait araÃ§ bulunamadÄ±.");
        }

        // --- KIYASLAMA METODU ---
        [HttpPost("compare")]
        public async Task<IActionResult> CompareCars([FromBody] List<CarCompareDto> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest("KÄ±yaslanacak araÃ§ bulunamadÄ±.");

            if (requests.Count > 4)
                return BadRequest("AynÄ± anda en fazla 4 araÃ§ kÄ±yaslayabilirsiniz.");

            var compareResults = new List<object>();

            foreach (var req in requests)
            {
                // Eğer özel bir donanım paketinden kıyaslamaya eklendiyse onu, değilse en güçlüsünü seçiyoruz
                var carQuery = _context.Cars
                    .Where(c => c.Make == req.Make &&
                                c.Model == req.Model &&
                                c.Generation == req.Generation &&
                                c.BodyType == req.BodyType);
                                
                if (!string.IsNullOrEmpty(req.Trim))
                {
                    carQuery = carQuery.Where(c => c.Trim == req.Trim);
                }

                var car = await carQuery
                    .OrderByDescending(c => c.HorsePower)
                    .ThenByDescending(c => c.EngineCC)
                    .FirstOrDefaultAsync();

                if (car != null)
                {
                    var gen = await _context.CarGenerations
                        .FirstOrDefaultAsync(g => g.Brand == req.Make &&
                                                  g.Model == req.Model &&
                                                  g.GenerationName == req.Generation &&
                                                  g.BodyType == req.BodyType);

                    compareResults.Add(new
                    {
                        make = car.Make,
                        model = car.Model,
                        generation = car.Generation,
                        year = car.Year,
                        bodyType = car.BodyType,
                        fuelType = car.FuelType,
                        transmission = car.Transmission,
                        engineCC = car.EngineCC,
                        horsePower = car.HorsePower,
                        acceleration = car.Acceleration,
                        maxSpeed = car.MaxSpeed,
                        averageFuelConsumption = car.AverageFuelConsumption,
                        trunkCapacity = car.TrunkCapacity,
                        length = car.Length,
                        width = car.Width,
                        height = car.Height,
                        averagePrice = gen?.AveragePrice ?? 0,
                        imageUrl = gen?.ImageUrl ?? "NotFound"
                    });
                }
            }

            if (!compareResults.Any())
                return NotFound("AraÃ§larÄ±n detaylarÄ±na ulaÅŸÄ±lamadÄ±.");

            return Ok(compareResults);
        }

        [HttpPost("queue-scraping")]
        public async Task<IActionResult> QueuePriceScraping([FromBody] CarPriceRequestDto request)
        {
            if (request.CarId <= 0 || string.IsNullOrEmpty(request.Make))
            {
                return BadRequest("GeÃ§ersiz araÃ§ bilgisi.");
            }

            string messageData = JsonSerializer.Serialize(request);

            using (var producer = new ProducerBuilder<Null, string>(_kafkaConfig).Build())
            {
                try
                {
                    var deliveryResult = await producer.ProduceAsync("car-price-scraping", new Message<Null, string> { Value = messageData });
                    Console.WriteLine($"[Kafka Producer] Mesaj fÄ±rlatÄ±ldÄ±: {deliveryResult.Value}");
                }
                catch (ProduceException<Null, string> e)
                {
                    return StatusCode(500, $"Kafka'ya mesaj gÃ¶nderilirken hata oluÅŸtu: {e.Error.Reason}");
                }
            }

            return Accepted(new
            {
                Message = "AraÃ§ fiyatÄ± Ã§ekme iÅŸlemi Kafka kuyruÄŸuna baÅŸarÄ±yla eklendi.",
                CarId = request.CarId
            });
        }

        [HttpPost("webhook/auto-add-car")]
        public async Task<IActionResult> AutoAddCarWebhook([FromBody] CarDetailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Make) || string.IsNullOrWhiteSpace(request.Model))
            {
                return BadRequest("Make and Model fields are required.");
            }

            var existingCar = await _context.Cars
                .FirstOrDefaultAsync(c => c.Make == request.Make && 
                                          c.Model == request.Model && 
                                          c.Generation == request.Generation && 
                                          c.EngineCC == request.EngineCC);

            if (existingCar != null)
            {
                return Ok(new { Message = "AraÃ§ zaten veritabanÄ±nda mevcut. Ä°ÅŸlem atlandÄ±.", CarId = existingCar.Id });
            }

            var newCar = new Car
            {
                Make = request.Make,
                Model = request.Model,
                Generation = request.Generation,
                Trim = request.Trim ?? "",
                Year = request.Year,
                BodyType = request.BodyType ?? "",
                Seats = request.Seats,
                HorsePower = request.HorsePower,
                EngineCC = request.EngineCC,
                Acceleration = request.Acceleration,
                MaxSpeed = request.MaxSpeed,
                FuelType = request.FuelType ?? "",
                Transmission = request.Transmission ?? "",
                CityFuelConsumption = request.CityFuelConsumption,
                HighwayFuelConsumption = 0,
                AverageFuelConsumption = request.AverageFuelConsumption,
                Length = 0, Width = 0, Height = 0, TrunkCapacity = 0
            };

            if (newCar.Seats == 0 || false) 
            {
                // EÄŸer yapay zeka koltuk sayÄ±sÄ±nÄ± bulamadÄ±ysa, kasa tipine gÃ¶re varsayÄ±m yap:
                if (newCar.BodyType.Contains("Hatchback", StringComparison.OrdinalIgnoreCase) || 
                    newCar.BodyType.Contains("Sedan", StringComparison.OrdinalIgnoreCase) ||
                    newCar.BodyType.Contains("SUV", StringComparison.OrdinalIgnoreCase))
                {
                    newCar.Seats = 5;
                }
                else if (newCar.BodyType.Contains("Coupe", StringComparison.OrdinalIgnoreCase))
                {
                    newCar.Seats = 4; // Coupeler genelde 4 kiÅŸiliktir
                }
                // BunlarÄ±n dÄ±ÅŸÄ±ndaysa 0 olarak kalmaya devam eder, arayÃ¼zde "Bilinmiyor" yazdÄ±rÄ±rsÄ±n.
            }
            await _context.Cars.AddAsync(newCar);
            
            var existingGen = await _context.CarGenerations
                .FirstOrDefaultAsync(g => g.Brand == request.Make && 
                                          g.Model == request.Model && 
                                          g.GenerationName == request.Generation && 
                                          g.BodyType == request.BodyType);
            if (existingGen == null)
            {
                var newGen = new CarGeneration
                {
                    Brand = request.Make,
                    Model = request.Model,
                    GenerationName = request.Generation,
                    BodyType = request.BodyType,
                    StartYear = request.Year,
                    EndYear = request.Year,
                    ImageUrl = !string.IsNullOrEmpty(request.ImageUrl) ? request.ImageUrl : "NotFound",
                    AveragePrice = 0
                };
                await _context.CarGenerations.AddAsync(newGen);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "AraÃ§ baÅŸarÄ±yla eklendi.", CarId = newCar.Id });
        }

    }
}
