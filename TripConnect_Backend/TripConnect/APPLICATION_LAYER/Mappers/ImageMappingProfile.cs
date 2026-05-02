using APPLICATION_LAYER.DTOs;
using AutoMapper;
using INFRASTRUCTURE_LAYER.Services;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for image-related mappings
    /// Maps between infrastructure layer responses and application layer DTOs
    /// </summary>
    public class ImageMappingProfile : Profile
    {
        public ImageMappingProfile()
        {
            // Map infrastructure ImageUploadResponse to application ImageUploadResponseDto
            CreateMap<ImageUploadResponse, ImageUploadResponseDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.PublicUrl, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.FileId))
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Success))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.ErrorMessage ?? "Image uploaded successfully"))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.UploadedAt));

            // Reverse mapping - from DTO to infrastructure response (if needed)
            CreateMap<ImageUploadResponseDto, ImageUploadResponse>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.Success ? null : src.Message))
                .ReverseMap();
        }
    }
}
