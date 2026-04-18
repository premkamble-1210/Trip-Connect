namespace APPLICATION_LAYER.DTOs.Trip
{
    /// <summary>
    /// Create Trip DTO
    /// </summary>
    public class CreateTripDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Seats { get; set; }
        public string TravelType { get; set; }
        public string? ImgUrl { get; set; }
    }
}
