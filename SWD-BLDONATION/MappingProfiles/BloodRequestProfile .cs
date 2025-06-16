using AutoMapper;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;

public class BloodRequestProfile : Profile
{
    public BloodRequestProfile()
    {
        CreateMap<BloodRequest, BloodRequestDto>();
        CreateMap<CreateBloodRequestDto, BloodRequest>();
        CreateMap<UpdateBloodRequestDto, BloodRequest>();
    }
}
