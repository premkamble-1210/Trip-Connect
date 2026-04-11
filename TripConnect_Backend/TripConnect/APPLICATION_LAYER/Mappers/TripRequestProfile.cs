using AutoMapper;
using APPLICATION_LAYER.DTOs.JoinRequest;
using DOMAIN_LAYER.Entity.TripRequest;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for TripRequest entity mappings
    /// </summary>
    public class TripRequestProfile : Profile
    {
        public TripRequestProfile()
        {
            // TripRequest → JoinRequestResponseDto
            CreateMap<TripRequest, JoinRequestResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // SendJoinRequestDto → TripRequest
            CreateMap<SendJoinRequestDto, TripRequest>()
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // UpdateJoinRequestDto → TripRequest
            CreateMap<UpdateJoinRequestDto, TripRequest>();
        }
    }
}
