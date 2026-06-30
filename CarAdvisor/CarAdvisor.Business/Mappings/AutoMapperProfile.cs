using AutoMapper;
using CarAdvisor.Domain.DTOs;
using CarAdvisor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.Business.Mappings;

public class AutoMapperProfile: Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Car, CarListDto>()
            .ForMember(dest => dest.BrandModel,
                       opt => opt.MapFrom(src => $"{src.Make} {src.Model}"))// Marka ve Modeli ayrı ayrı değil, birleşik yaxar.
            .ForMember(dest => dest.Package,
                       opt => opt.MapFrom(src => src.Trim))// Trim -> Package
            .ForMember(dest => dest.Fuel,
                       opt => opt.MapFrom(src => src.FuelType))// FuelType -> Fuel
            .ForMember(dest => dest.Gear,
                       opt => opt.MapFrom(src => src.Transmission))// Transmission -> Gear
            .ForMember(dest => dest.Consumption,
                       opt => opt.MapFrom(src => src.AverageFuelConsumption));// AverageFuelConsumption -> Consumption

    }

}
