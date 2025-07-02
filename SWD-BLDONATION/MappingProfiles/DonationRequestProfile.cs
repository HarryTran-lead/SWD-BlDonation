using AutoMapper;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.Models.Generated;

namespace SWD_BLDONATION.Mapping
{
    public class DonationRequestProfile : Profile
    {
        public DonationRequestProfile()
        {
            // Map từ Entity → DTO (đọc dữ liệu)
            CreateMap<DonationRequest, DonationRequestDto>()
                .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src => src.BloodType))  // navigation
                .ForMember(dest => dest.BloodComponent, opt => opt.MapFrom(src => src.BloodComponent))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // Map từ Create DTO → Entity (tạo mới)
            CreateMap<CreateDonationRequestDto, DonationRequest>()
                .ForMember(dest => dest.DonateRequestId, opt => opt.Ignore())
                // KHÔNG map BloodType, BloodComponent (navigation)
                .ForMember(dest => dest.BloodType, opt => opt.Ignore())
                .ForMember(dest => dest.BloodComponent, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.RequestMatches, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Map từ Update DTO → Entity (cập nhật)
            CreateMap<UpdateDonationRequestDto, DonationRequest>()
                .ForMember(dest => dest.DonateRequestId, opt => opt.Ignore())
                .ForMember(dest => dest.BloodType, opt => opt.Ignore())
                .ForMember(dest => dest.BloodComponent, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.RequestMatches, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Không sửa CreatedAt khi update
        }
    }
}
