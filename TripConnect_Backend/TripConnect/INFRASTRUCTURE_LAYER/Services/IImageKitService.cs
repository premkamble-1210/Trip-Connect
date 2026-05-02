using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Interface for ImageKit image upload and management operations
    /// Handles all external ImageKit API interactions
    /// </summary>
    public interface IImageKitService
    {
        /// <summary>
        /// Upload an image to ImageKit
        /// </summary>
        /// <param name="imageStream">Stream of the image file</param>
        /// <param name="fileName">Name of the file to upload</param>
        /// <param name="folder">Folder path in ImageKit where image will be stored</param>
        /// <returns>ImageUploadResponse containing file details and URLs</returns>
        Task<ImageUploadResponse> UploadImageAsync(Stream imageStream, string fileName, string folder = "uploads");

        /// <summary>
        /// Delete an image from ImageKit by file ID
        /// </summary>
        /// <param name="fileId">ImageKit file ID to delete</param>
        /// <returns>True if deletion successful, false otherwise</returns>
        Task<bool> DeleteImageAsync(string fileId);

        /// <summary>
        /// Generate an optimized image URL from ImageKit with optional transformations
        /// </summary>
        /// <param name="filePath">Path of the file in ImageKit</param>
        /// <param name="width">Optional width for image resizing</param>
        /// <param name="height">Optional height for image resizing</param>
        /// <param name="quality">Optional quality parameter (1-100)</param>
        /// <returns>Optimized image URL</returns>
        Task<string> GetImageUrlAsync(string filePath, int? width = null, int? height = null, int? quality = null);
    }

    /// <summary>
    /// Response model for image upload operations
    /// </summary>
    public class ImageUploadResponse
    {
        /// <summary>
        /// Unique identifier for the uploaded file in ImageKit
        /// </summary>
        public string FileId { get; set; } = string.Empty;

        /// <summary>
        /// Full URL of the uploaded image
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// File path in ImageKit
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Original file name
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Image width in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Image height in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Timestamp when image was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Indicates if the upload was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if upload failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
