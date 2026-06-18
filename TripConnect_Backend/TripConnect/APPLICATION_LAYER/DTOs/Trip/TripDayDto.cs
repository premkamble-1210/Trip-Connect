namespace APPLICATION_LAYER.DTOs.Trip
{
    /// <summary>
    /// TripDay input DTO — used in CreateTripDto and UpdateTripDto
    /// </summary>
    public class TripDayDto
    {
        public int Day { get; set; }
        public string? Location { get; set; }
        public DateOnly Date { get; set; }
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
