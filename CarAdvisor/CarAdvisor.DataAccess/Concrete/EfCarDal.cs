using CarAdvisor.DataAccess.Abstract;
using CarAdvisor.DataAccess.Contexts;
using CarAdvisor.Domain.DTOs;
using CarAdvisor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarAdvisor.DataAccess.Concrete;

public class EfCarDal : ICarDal
{
    public List<Car> GetAll()
    {
        using (var context = new CarAdvisorContext())
            return context.Cars.Take(50).ToList();
    }

    public Car GetById(int id)
    {
        using (var context = new CarAdvisorContext())
            return context.Cars.FirstOrDefault(c => c.Id == id);
    }

    public List<string> GetBrands()
    {
        using (var context = new CarAdvisorContext())
        {
            return context.Cars
                .Select(c => c.Make)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
        }
    }

    // --- GÜNCELLENEN METOT ---
    public List<CarGeneration> GetGenerationsByBrand(string brand)
    {
        using (var context = new CarAdvisorContext())
        {
            // 1. Veriyi çek ve tarihe göre sırala
            var generations = context.CarGenerations
                .Where(x => x.Brand == brand)
                .OrderBy(x => x.Model)       // Önce Model (A3, Megane)
                .ThenBy(x => x.StartYear)    // Sonra Yıl (Eskiden yeniye)
                .ToList();

            // 2. AKILLI YIL HESAPLAMA (Algoritmayı buraya gömdük)
            // Verileri model bazında gruplayıp boşlukları dolduruyoruz.
            var groupedByModel = generations.GroupBy(x => x.Model);

            foreach (var group in groupedByModel)
            {
                var modelList = group.ToList(); // Örn: Sadece Megane listesi

                for (int i = 0; i < modelList.Count; i++)
                {
                    var currentGen = modelList[i];

                    // Eğer bitiş yılı veritabanında "aynı" veya 0 ise (Eksik Veri)
                    // VE bir sonraki jenerasyon varsa:
                    if ((currentGen.EndYear == currentGen.StartYear || currentGen.EndYear == 0)
                         && i < modelList.Count - 1)
                    {
                        var nextGen = modelList[i + 1];

                        // Bitiş yılını, bir sonraki kasanın başlangıç yılı yapıyoruz.
                        currentGen.EndYear = nextGen.StartYear;
                    }

                    // Eğer bu son jenerasyonsa ve yılı günümüze yakınsa (örn: 2020 sonrası)
                    // "Günümüz" yazması için EndYear'ı null veya 0 bırakabiliriz.
                    if (i == modelList.Count - 1 && currentGen.StartYear >= DateTime.Now.Year - 5)
                    {
                        // Buraya müdahale etmiyoruz, zaten null/0 geliyorsa React "Günümüz" yazar.
                    }
                }
            }

            return generations;
        }
    }
    public List<CarDetailDto> GetCarDetails(string brand, string model, string generation, string bodyType)
    {
        using (var context = new CarAdvisorContext())
        {
            string b = brand?.Trim().ToLower() ?? "";
            string m = model?.Trim().ToLower() ?? "";
            string g = generation?.Trim().ToLower() ?? "";
            string bt = bodyType?.Trim().ToLower() ?? "";

            // EĞER REACT'TEN GELEN KASA TİPİ BOŞSA, "-" İSE VEYA "NULL" YAZIYORSA BUNU TESPİT ET
            bool isBodyTypeEmpty = string.IsNullOrEmpty(bt) || bt == "-" || bt == "null" || bt == "undefined";

            var result = (from c in context.Cars
                          join cg in context.CarGenerations
                          on new { Make = c.Make, Model = c.Model, Generation = c.Generation, BodyType = c.BodyType }
                          equals new { Make = cg.Brand, Model = cg.Model, Generation = cg.GenerationName, BodyType = cg.BodyType } into joinedData
                          from cgResult in joinedData.DefaultIfEmpty()

                          where c.Make.ToLower().Contains(b) &&
                                c.Model.ToLower().Contains(m) &&
                                c.Generation.ToLower().Contains(g) &&
                                // BÜYÜK DOKUNUŞ BURASI: Kasa tipi boşsa filtreleme, direkt getir! 
                                // Doluysa da içinde kelime geçiyor mu diye bak.
                                (isBodyTypeEmpty || (c.BodyType != null && c.BodyType.ToLower().Contains(bt)))

                          select new CarDetailDto
                          {
                              Id = c.Id,
                              Make = c.Make,
                              Model = c.Model,
                              Generation = c.Generation,
                              Trim = c.Trim,
                              Year = c.Year,
                              BodyType = c.BodyType,
                              Seats = c.Seats,
                              HorsePower = c.HorsePower,
                              EngineCC = c.EngineCC,
                              Acceleration = c.Acceleration,
                              MaxSpeed = c.MaxSpeed,
                              FuelType = c.FuelType,
                              Transmission = c.Transmission,
                              CityFuelConsumption = c.CityFuelConsumption,
                              AverageFuelConsumption = c.AverageFuelConsumption,
                              AveragePrice = cgResult != null ? cgResult.AveragePrice : null,
                              ImageUrl = cgResult != null ? cgResult.ImageUrl : null
                          }).ToList();

            return result;
        }
    }
    public List<CarGenerationDto> GetGenerationsByBodyType(string bodyType)
    {
        using (var context = new CarAdvisorContext())
        {
            var result = from cg in context.CarGenerations
                         where cg.BodyType.ToLower() == bodyType.ToLower()
                         select new CarGenerationDto
                         {
                             Id = cg.Id,
                             Brand = cg.Brand,
                             Model = cg.Model,
                             GenerationName = cg.GenerationName,
                             StartYear = cg.StartYear,
                             EndYear = cg.EndYear,
                             BodyType = cg.BodyType,
                             AveragePrice = cg.AveragePrice,
                             ImageUrl = cg.ImageUrl
                         };
            return result.ToList();
        }
    }
}