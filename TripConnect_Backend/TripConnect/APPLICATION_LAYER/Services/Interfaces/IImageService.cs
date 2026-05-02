using APPLICATION_LAYER.DTOs;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Service interface for image management operations
    /// Handles image uploads, deletions, and URL transformations
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Upload an image for a user profile
        /// </summary>
        /// <param name="imageStream">Image file stream to upload</param>
        /// <param name="fileName">File name of the image</param>
        /// <param name="userId">User ID who owns the image</param>
        /// <returns>Image upload response with file ID and URLs</returns>
        Task<ImageUploadResponseDto> UploadProfileImageAsync(Stream imageStream, string fileName, int userId);

        /// <summary>
        /// Upload an image for a trip
        /// </summary>
        /// <param name="imageStream">Image file stream to upload</param>
        /// <param name="fileName">File name of the image</param>
        /// <param name="tripId">Trip ID associated with the image</param>
        /// <returns>Image upload response with file ID and URLs</returns>
        Task<ImageUploadResponseDto> UploadTripImageAsync(Stream imageStream, string fileName, int tripId);

        /// <summary>
        /// Upload an image for an expense
        /// </summary>
        /// <param name="imageStream">Image file stream to upload</param>
        /// <param name="fileName">File name of the image</param>
        /// <param name="expenseId">Expense ID associated with the image</param>
        /// <returns>Image upload response with file ID and URLs</returns>
        Task<ImageUploadResponseDto> UploadExpenseImageAsync(Stream imageStream, string fileName, int expenseId);

        /// <summary>
        /// Delete an image by file ID
        /// </summary>
        /// <param name="fileId">Unique file identifier to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteImageAsync(string fileId);

        /// <summary>
        /// Extract file ID from an image URL
        /// </summary>
        /// <param name="imageUrl">Full image URL</param>
        /// <returns>Extracted file ID from the URL</returns>
        Task<string> GetFileIdFromUrlAsync(string imageUrl);

        /// <summary>
        /// Get image URL from file ID
        /// </summary>
        /// <param name="fileId">Unique file identifier</param>
        /// <returns>Constructed image URL for the file ID</returns>
        Task<string> GetUrlFromFileIdAsync(string fileId);
    }
}
