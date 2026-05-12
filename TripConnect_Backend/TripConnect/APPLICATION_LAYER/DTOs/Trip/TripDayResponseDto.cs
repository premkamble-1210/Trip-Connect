namespace APPLICATION_LAYER.DTOs.Trip
{
    /// <summary>
    /// TripDay response DTO — returned inside TripResponseDto
    /// </summary>
    public class TripDayResponseDto
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int Day { get; set; }
        public string? Location { get; set; }
        public DateOnly Date { get; set; }
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
