using Microsoft.AspNetCore.Http;

namespace API_LAYER.Models
{
    /// <summary>
    /// Form data model for image upload endpoints
    /// </summary>
    public class ImageUploadFormDto
    {
        /// <summary>
        /// Image file to upload
        /// </summary>
        public IFormFile? ImageFile { get; set; }
    }
}