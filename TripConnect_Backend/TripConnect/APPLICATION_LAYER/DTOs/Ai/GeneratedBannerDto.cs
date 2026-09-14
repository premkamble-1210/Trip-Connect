namespace APPLICATION_LAYER.DTOs.Ai
{
    /// <summary>
    /// AI-generated trip banner. <see cref="ImgUrl"/> is the persistable,
    /// CDN-backed image URL (ImageKit when possible, raw generation URL as fallback).
    /// </summary>
    public class GeneratedBannerDto
    {
        public string ImgUrl { get; set; } = string.Empty;
        public string Source { get; set; } = "pollinations";
    }
}