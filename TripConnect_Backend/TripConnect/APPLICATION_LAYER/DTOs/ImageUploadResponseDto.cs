namespace APPLICATION_LAYER.DTOs
{
    /// <summary>
    /// Data Transfer Object for image upload responses
    /// </summary>
    public class ImageUploadResponseDto
    {
        /// <summary>
        /// Unique identifier for the uploaded image
        /// </summary>
        public string FileId { get; set; } = string.Empty;

        /// <summary>
        /// URL to access the uploaded image
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Public URL for the image
        /// </summary>
        public string PublicUrl { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when image was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether upload was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success or error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Image width in pixels
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Image height in pixels
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// File name of uploaded image
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}
