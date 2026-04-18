namespace APPLICATION_LAYER.DTOs.TripMember
{
    /// <summary>
    /// Trip Member Response DTO
    /// </summary>
    public class TripMemberResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
