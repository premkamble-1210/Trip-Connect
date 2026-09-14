namespace APPLICATION_LAYER.DTOs.Ai
{
    /// <summary>
    /// Request DTO for AI trip banner generation.
    /// </summary>
    public class GenerateBannerDto
    {
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}