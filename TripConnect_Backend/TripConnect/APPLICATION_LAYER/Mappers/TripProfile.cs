using AutoMapper;
using APPLICATION_LAYER.DTOs.Trip;
using DOMAIN_LAYER.Entity.Trip;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for Trip entity mappings
    /// </summary>
    public class TripProfile : Profile
    {
        public TripProfile()
        {
            // Trip → TripResponseDto
            CreateMap<Trip, TripResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // CreateTripDto → Trip
            CreateMap<CreateTripDto, Trip>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImgUrl, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ImgUrl)));

            // UpdateTripDto → Trip
            CreateMap<UpdateTripDto, Trip>()
                .ForMember(dest => dest.ImgUrl, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ImgUrl)));
        }
    }
}
