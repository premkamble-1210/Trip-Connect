namespace APPLICATION_LAYER.DTOs.Rating
{
    /// <summary>
    /// Rating Response DTO
    /// </summary>
    public class RatingResponseDto
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int RatedBy { get; set; }
        public string RatedByName { get; set; }
        public int RatedUserId { get; set; }
        public string RatedUserName { get; set; }
        public double Rating { get; set; }
        public string Review { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
