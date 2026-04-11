namespace APPLICATION_LAYER.DTOs.Rating
{
    /// <summary>
    /// Create Rating DTO
    /// </summary>
    public class CreateRatingDto
    {
        public int TripId { get; set; }
        public int RatedUserId { get; set; }
        public double Rating { get; set; } // 1.0 to 5.0
        public string Review { get; set; }
    }
}
