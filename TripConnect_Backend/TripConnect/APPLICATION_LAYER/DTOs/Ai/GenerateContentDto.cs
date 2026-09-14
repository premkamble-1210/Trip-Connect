namespace APPLICATION_LAYER.DTOs.Ai
{
    /// <summary>
    /// Request DTO for AI-assisted trip title/description generation.
    /// </summary>
    public class GenerateContentDto
    {
        public string Location { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TravelType { get; set; } = string.Empty;
        public int? Seats { get; set; }
    }
}