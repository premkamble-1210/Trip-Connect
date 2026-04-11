using AutoMapper;
using APPLICATION_LAYER.DTOs.Rating;
using DOMAIN_LAYER.Entity.TripRating;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for TripRating entity mappings
    /// </summary>
    public class TripRatingProfile : Profile
    {
        public TripRatingProfile()
        {
            // TripRating → RatingResponseDto
            CreateMap<TripRating, RatingResponseDto>();

            // CreateRatingDto → TripRating
            CreateMap<CreateRatingDto, TripRating>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
