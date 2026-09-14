using APPLICATION_LAYER.DTOs.Trip;

namespace APPLICATION_LAYER.DTOs.Ai
{
    /// <summary>
    /// AI-generated day-by-day itinerary. Returns the same TripDayDto shape
    /// the create-trip form sends, so days can be previewed/edited before saving.
    /// </summary>
    public class GeneratedItineraryDto
    {
        public List<TripDayDto> TripDays { get; set; } = new();
    }
}