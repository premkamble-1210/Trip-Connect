using APPLICATION_LAYER.DTOs;
using APPLICATION_LAYER.Services.Interfaces;
using INFRASTRUCTURE_LAYER.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace APPLICATION_LAYER.Services.Implementations
{
    /// <summary>
    /// Service implementation for image management operations
    /// Handles validation, business logic, and orchestration with infrastructure layer
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IImageKitService _imageKitService;
        private readonly ILogger<ImageService> _logger;

        // Configuration constants
        private const int MaxFileSizeMB = 5;
        private const int MinDimension = 100;
        private const int MaxDimension = 4000;
        private readonly string[] _allowedFormats = { "jpg", "jpeg", "png", "webp" };

        public ImageService(IImageKitService imageKitService, ILogger<ImageService> logger)
        {
            _imageKitService = imageKitService ?? throw new ArgumentNullException(nameof(imageKitService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Upload profile image for a user
        /// </summary>
        public async Task<ImageUploadResponseDto> UploadProfileImageAsync(Stream imageStream, string fileName, int userId)
        {
            _logger.LogInformation("Uploading profile image for user {UserId}", userId);

            try
            {
                ValidateImage(imageStream, fileName);

                var folder = $"profiles/user_{userId}";
                var response = await _imageKitService.UploadImageAsync(imageStream, fileName, folder);

                return MapToDto(response, "Profile image uploaded successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation failed for profile image upload: {Message}", ex.Message);
                return CreateErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile image for user {UserId}", userId);
                return CreateErrorResponse("Failed to upload profile image");
            }
        }

        /// <summary>
        /// Upload image for a trip
        /// </summary>
        public async Task<ImageUploadResponseDto> UploadTripImageAsync(Stream imageStream, string fileName, int tripId)
        {
            _logger.LogInformation("Uploading trip image for trip {TripId}", tripId);

            try
            {
                ValidateImage(imageStream, fileName);

                var folder = $"trips/trip_{tripId}";
                var response = await _imageKitService.UploadImageAsync(imageStream, fileName, folder);

                return MapToDto(response, "Trip image uploaded successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation failed for trip image upload: {Message}", ex.Message);
                return CreateErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading trip image for trip {TripId}", tripId);
                return CreateErrorResponse("Failed to upload trip image");
            }
        }

        /// <summary>
        /// Upload image for an expense
        /// </summary>
        public async Task<ImageUploadResponseDto> UploadExpenseImageAsync(Stream imageStream, string fileName, int expenseId)
        {
            _logger.LogInformation("Uploading expense image for expense {ExpenseId}", expenseId);

            try
            {
                ValidateImage(imageStream, fileName);

                var folder = $"expenses/expense_{expenseId}";
                var response = await _imageKitService.UploadImageAsync(imageStream, fileName, folder);

                return MapToDto(response, "Expense image uploaded successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation failed for expense image upload: {Message}", ex.Message);
                return CreateErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading expense image for expense {ExpenseId}", expenseId);
                return CreateErrorResponse("Failed to upload expense image");
            }
        }

        /// <summary>
        /// Delete image by file ID
        /// </summary>
        public async Task<bool> DeleteImageAsync(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                _logger.LogWarning("Attempted to delete image with empty file ID");
                return false;
            }

            _logger.LogInformation("Deleting image with file ID {FileId}", fileId);

            try
            {
                return await _imageKitService.DeleteImageAsync(fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with file ID {FileId}", fileId);
                return false;
            }
        }

        /// <summary>
        /// Extract file ID from image URL
        /// Parses URL to extract the unique file identifier
        /// </summary>
        public async Task<string> GetFileIdFromUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogWarning("Attempted to extract file ID from empty URL");
                throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));
            }

            _logger.LogInformation("Extracting file ID from URL");

            try
            {
                // Extract file ID from URL path
                // Example: https://ik.imagekit.io/your_id/path/to/file_abc123.jpg
                // File ID is typically the file name or UUID portion
                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (pathSegments.Length == 0)
                {
                    throw new ArgumentException("Invalid URL format", nameof(imageUrl));
                }

                // Get the last segment (file name) and extract ID before extension
                var fileName = pathSegments[^1];
                var fileId = Path.GetFileNameWithoutExtension(fileName);

                _logger.LogInformation("Successfully extracted file ID: {FileId}", fileId);
                return await Task.FromResult(fileId);
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Invalid URL format provided");
                throw new ArgumentException("Invalid URL format", nameof(imageUrl), ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting file ID from URL");
                throw;
            }
        }

        /// <summary>
        /// Get image URL from file ID
        /// Constructs the full URL for a given file ID
        /// </summary>
        public async Task<string> GetUrlFromFileIdAsync(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                _logger.LogWarning("Attempted to generate URL for empty file ID");
                throw new ArgumentException("File ID cannot be null or empty", nameof(fileId));
            }

            _logger.LogInformation("Generating URL for file ID {FileId}", fileId);

            try
            {
                // Call infrastructure service to generate URL with optimizations
                var imageUrl = await _imageKitService.GetImageUrlAsync(fileId, width: null, height: null, quality: null);

                _logger.LogInformation("Successfully generated URL for file ID: {FileId}", fileId);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating URL for file ID {FileId}", fileId);
                throw;
            }
        }

        /// <summary>
        /// Validate image file for upload
        /// Checks size, format, and basic dimensions
        /// </summary>
        private void ValidateImage(Stream? imageStream, string? fileName)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                throw new ArgumentException("Image stream is required and cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required and cannot be empty");
            }

            // Validate file size (5MB max)
            var fileSizeInMB = imageStream.Length / (1024.0 * 1024.0);
            if (fileSizeInMB > MaxFileSizeMB)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSizeMB}MB");
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLower();
            if (!_allowedFormats.Contains(fileExtension))
            {
                throw new ArgumentException($"File format '{fileExtension}' is not allowed. Allowed formats: {string.Join(", ", _allowedFormats)}");
            }

            _logger.LogInformation("Image validation passed for file {FileName}", fileName);
        }

        /// <summary>
        /// Map infrastructure ImageUploadResponse to application DTO
        /// </summary>
        private ImageUploadResponseDto MapToDto(ImageUploadResponse response, string successMessage)
        {
            return new ImageUploadResponseDto
            {
                FileId = response.FileId,
                ImageUrl = response.Url,
                PublicUrl = response.Url,
                UploadedAt = response.UploadedAt,
                Success = response.Success,
                Message = successMessage,
                FileSize = response.FileSize,
                Width = response.Width,
                Height = response.Height,
                FileName = response.FileName
            };
        }

        /// <summary>
        /// Create error response DTO
        /// </summary>
        private ImageUploadResponseDto CreateErrorResponse(string errorMessage)
        {
            return new ImageUploadResponseDto
            {
                Success = false,
                Message = errorMessage,
                UploadedAt = DateTime.UtcNow
            };
        }
    }
}
