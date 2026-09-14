namespace APPLICATION_LAYER.DTOs.Ai
{
    /// <summary>
    /// Request DTO for AI itinerary generation.
    /// </summary>
    public class GenerateItineraryDto
    {
        public string Location { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TravelType { get; set; } = string.Empty;
    }
}