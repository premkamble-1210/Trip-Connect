namespace APPLICATION_LAYER.DTOs.JoinRequest
{
    /// <summary>
    /// Join Request Response DTO
    /// </summary>
    public class JoinRequestResponseDto
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
