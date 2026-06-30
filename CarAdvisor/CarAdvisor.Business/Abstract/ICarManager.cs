using CarAdvisor.Domain.DTOs;
using CarAdvisor.Domain.Entities;
using System.Globalization;


namespace CarAdvisor.Business.Abstract;

public interface ICarManager
{
    List<CarListDto> GetAll();
    List<string> GetBrands();
    List<CarGeneration> GetGenerationsByBrand(string brand);
    List<CarDetailDto> GetCarDetails(string brand, string model, string generation, string bodyType);
    List<CarGenerationDto> GetGenerationsByBodyType(string bodyType);
}
