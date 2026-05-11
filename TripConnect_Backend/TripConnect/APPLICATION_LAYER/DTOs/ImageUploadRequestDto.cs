namespace APPLICATION_LAYER.DTOs
{
    /// <summary>
    /// Data Transfer Object for image upload requests
    /// </summary>
    public class ImageUploadRequestDto
    {
        /// <summary>
        /// Image file stream to upload
        /// </summary>
        public Stream? ImageStream { get; set; }

        /// <summary>
        /// File name of the image
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Folder path for organizing uploads (e.g., "profiles", "trips", "expenses")
        /// Default: "uploads"
        /// </summary>
        public string Folder { get; set; } = "uploads";

        /// <summary>
        /// Optional description for the image
        /// </summary>
        public string? Description { get; set; }
    }
}
