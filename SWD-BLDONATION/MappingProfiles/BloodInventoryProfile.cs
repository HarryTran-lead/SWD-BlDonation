using AutoMapper;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.DTOs.BloodInventoryDTOs;

namespace SWD_BLDONATION.MappingProfiles
{
    public class BloodInventoryProfile : Profile
    {
        public BloodInventoryProfile()
        {
            CreateMap<BloodInventory, BloodInventoryDto>().ReverseMap();
            CreateMap<CreateBloodInventoryDto, BloodInventory>();
            CreateMap<UpdateBloodInventoryDto, BloodInventory>();
        }
    }
}
