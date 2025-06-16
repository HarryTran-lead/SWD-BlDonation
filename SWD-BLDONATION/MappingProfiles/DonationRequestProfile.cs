using AutoMapper;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.Models.Generated;

namespace SWD_BLDONATION.Mapping
{
    public class DonationRequestProfile : Profile
    {
        public DonationRequestProfile()
        {
            // Mapping from DonationRequest to DonationRequestDto
            CreateMap<DonationRequest, DonationRequestDto>()
                .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src => src.BloodType))
                .ForMember(dest => dest.BloodComponent, opt => opt.MapFrom(src => src.BloodComponent))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // Mapping from CreateDonationRequestDto to DonationRequest
            CreateMap<CreateDonationRequestDto, DonationRequest>()
                .ForMember(dest => dest.DonateRequestId, opt => opt.Ignore())  // Ignore DonateRequestId as it will be auto-generated
                .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src => src.BloodTypeId))  // Mapping BloodTypeId to BloodType
                .ForMember(dest => dest.BloodComponent, opt => opt.MapFrom(src => src.BloodComponentId));  // Mapping BloodComponentId to BloodComponent

            // Mapping from UpdateDonationRequestDto to DonationRequest
            CreateMap<UpdateDonationRequestDto, DonationRequest>()
                .ForMember(dest => dest.DonateRequestId, opt => opt.Ignore())  // Ignore DonateRequestId as it's not needed
                .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src => src.BloodTypeId))  // Mapping BloodTypeId to BloodType
                .ForMember(dest => dest.BloodComponent, opt => opt.MapFrom(src => src.BloodComponentId));  // Mapping BloodComponentId to BloodComponent
        }
    }
}
