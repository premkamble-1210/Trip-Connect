using AutoMapper;
using APPLICATION_LAYER.DTOs.User;
using DOMAIN_LAYER.Entity.User;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for User entity mappings
    /// </summary>
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // User → UserResponseDto
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => src.PhoneVerified))
                .ForMember(dest => dest.IdVerified, opt => opt.MapFrom(src => src.IdVerified))
                .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => src.EmailVerified))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // CreateUserDto → User
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IdVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 0.0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // UpdateUserDto → User (only maps provided fields)
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Name, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Name)))
                .ForMember(dest => dest.Phone, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Phone)));
        }
    }
}
