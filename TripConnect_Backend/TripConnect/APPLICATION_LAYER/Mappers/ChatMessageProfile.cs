using AutoMapper;
using APPLICATION_LAYER.DTOs.Chat;
using DOMAIN_LAYER.Entity.ChatMessage;

namespace APPLICATION_LAYER.Mappers
{
    /// <summary>
    /// AutoMapper profile for ChatMessage entity mappings
    /// </summary>
    public class ChatMessageProfile : Profile
    {
        public ChatMessageProfile()
        {
            // ChatMessage → ChatMessageResponseDto
            CreateMap<ChatMessage, ChatMessageResponseDto>();

            // SendMessageDto → ChatMessage
            CreateMap<SendMessageDto, ChatMessage>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
