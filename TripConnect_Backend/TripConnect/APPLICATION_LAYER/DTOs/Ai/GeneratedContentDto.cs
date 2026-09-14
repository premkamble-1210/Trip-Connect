namespace APPLICATION_LAYER.DTOs.Ai
{
    /// <summary>
    /// AI-generated trip title and description.
    /// </summary>
    public class GeneratedContentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}