namespace APPLICATION_LAYER.DTOs.Chat
{
    /// <summary>
    /// Chat Message Response DTO
    /// </summary>
    public class ChatMessageResponseDto
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
