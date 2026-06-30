using CarAdvisor.Business.Abstract;
using CarAdvisor.DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CarAdvisor.Domain.DTOs;
using CarAdvisor.DataAccess.Contexts;
using CarAdvisor.Domain.Entities;
namespace CarAdvisor.Business.Concrete;

public class CarManager:ICarManager
{
    private readonly ICarDal _carDal;
    private readonly IMapper _carMapper;

    public CarManager(ICarDal carDal, IMapper carMapper)
    {
        _carDal = carDal;
        _carMapper = carMapper;
    }


    public List<CarListDto> GetAll()
    {
        // 1. Veritabanından ham veriyi çek (Entity Listesi)
        var cars= _carDal.GetAll();

        // 2. Ham veriyi DTO'ya dönüştür (Mapping)

        return _carMapper.Map<List<CarListDto>>(cars);
    }

    public List<string> GetBrands()
    {
        return _carDal.GetBrands();
    }

    public List<CarGeneration> GetGenerationsByBrand(string brand)
    {
        var generations = _carDal.GetGenerationsByBrand(brand);
        return new List<CarGeneration>(generations);
    }

    public List<CarDetailDto> GetCarDetails(string brand, string model, string generation, string bodyType)
    {
        return _carDal.GetCarDetails(brand, model, generation,bodyType);
    }
    public List<CarGenerationDto> GetGenerationsByBodyType(string bodyType)
    {
        return _carDal.GetGenerationsByBodyType(bodyType);
    }
}
