namespace APPLICATION_LAYER.DTOs.Trip
{
    /// <summary>
    /// Trip Response DTO - Return trip info
    /// </summary>
    public class TripResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Seats { get; set; }
        public string TravelType { get; set; }
        public string ImgUrl { get; set; }
        public string Status { get; set; }
        public int HostId { get; set; }
        public string HostName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TripDayResponseDto> TripDays { get; set; } = new();
    }
}
