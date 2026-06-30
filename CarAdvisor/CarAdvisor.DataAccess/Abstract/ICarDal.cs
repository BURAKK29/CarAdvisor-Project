using CarAdvisor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarAdvisor.Domain.DTOs;

namespace CarAdvisor.DataAccess.Abstract;

public interface ICarDal
{
    List<Car> GetAll();
    Car GetById(int id);

    List<string> GetBrands();
    public List<CarGeneration> GetGenerationsByBrand(string brand);
    List<CarDetailDto> GetCarDetails(string brand, string model, string generation, string bodyType);
    List<CarGenerationDto> GetGenerationsByBodyType(string bodyType);
}
