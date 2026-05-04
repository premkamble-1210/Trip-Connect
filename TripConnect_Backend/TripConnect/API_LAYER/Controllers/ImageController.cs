using APPLICATION_LAYER.DTOs;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Image Controller - Handles image upload, deletion, and URL transformations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Upload a profile image for the authenticated user
        /// </summary>
        /// <param name="imageFile">Image file to upload</param>
        /// <returns>Image upload response with file ID and URLs</returns>
        [HttpPost("upload-profile")]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No image file provided" });
                }

                // Extract userId from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid user claim for profile image upload");
                    return Unauthorized(new { success = false, message = "Invalid user claim" });
                }

                _logger.LogInformation("Uploading profile image for user {UserId}, file: {FileName}", userId, imageFile.FileName);

                // Convert IFormFile to Stream
                await using var stream = imageFile.OpenReadStream();
                var response = await _imageService.UploadProfileImageAsync(stream, imageFile.FileName, userId);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error uploading profile image");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile image");
                return StatusCode(500, new { success = false, message = "Failed to upload profile image" });
            }
        }

        /// <summary>
        /// Upload an image for a trip
        /// </summary>
        /// <param name="tripId">Trip ID to associate the image with</param>
        /// <param name="imageFile">Image file to upload</param>
        /// <returns>Image upload response with file ID and URLs</returns>
        [HttpPost("upload-trip/{tripId}")]
        public async Task<IActionResult> UploadTripImage([FromRoute] int tripId, [FromForm] IFormFile imageFile)
        {
            try
            {
                if (tripId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid trip ID" });
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No image file provided" });
                }

                _logger.LogInformation("Uploading trip image for trip {TripId}, file: {FileName}", tripId, imageFile.FileName);

                // Convert IFormFile to MemoryStream to avoid position issues
                await using var originalStream = imageFile.OpenReadStream();
                await using var memoryStream = new MemoryStream();
                await originalStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Ensure clean start
                
                var response = await _imageService.UploadTripImageAsync(memoryStream, imageFile.FileName, tripId);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error uploading trip image");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading trip image");
                return StatusCode(500, new { success = false, message = "Failed to upload trip image" });
            }
        }

        /// <summary>
        /// Upload an image for an expense
        /// </summary>
        /// <param name="expenseId">Expense ID to associate the image with</param>
        /// <param name="imageFile">Image file to upload</param>
        /// <returns>Image upload response with file ID and URLs</returns>
        [HttpPost("upload-expense/{expenseId}")]
        public async Task<IActionResult> UploadExpenseImage([FromRoute] int expenseId, [FromForm] IFormFile imageFile)
        {
            try
            {
                if (expenseId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid expense ID" });
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No image file provided" });
                }

                _logger.LogInformation("Uploading expense image for expense {ExpenseId}, file: {FileName}", expenseId, imageFile.FileName);

                // Convert IFormFile to Stream
                await using var stream = imageFile.OpenReadStream();
                var response = await _imageService.UploadExpenseImageAsync(stream, imageFile.FileName, expenseId);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error uploading expense image");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading expense image");
                return StatusCode(500, new { success = false, message = "Failed to upload expense image" });
            }
        }

        /// <summary>
        /// Delete an image by file ID
        /// </summary>
        /// <param name="fileId">Unique file identifier to delete</param>
        /// <returns>Success/failure response</returns>
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteImage([FromRoute] string fileId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                {
                    return BadRequest(new { success = false, message = "File ID is required" });
                }

                _logger.LogInformation("Deleting image with file ID: {FileId}", fileId);

                var success = await _imageService.DeleteImageAsync(fileId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Image not found or deletion failed" });
                }

                return Ok(new { success = true, message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with file ID: {FileId}", fileId);
                return StatusCode(500, new { success = false, message = "Failed to delete image" });
            }
        }

        /// <summary>
        /// Extract file ID from an image URL
        /// </summary>
        /// <param name="imageUrl">Full image URL (URL-encoded)</param>
        /// <returns>Extracted file ID</returns>
        [HttpGet("file-id")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFileIdFromUrl([FromQuery] string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return BadRequest(new { success = false, message = "Image URL is required" });
                }

                _logger.LogInformation("Extracting file ID from URL");

                var fileId = await _imageService.GetFileIdFromUrlAsync(imageUrl);

                return Ok(new { success = true, fileId = fileId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid URL format");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting file ID from URL");
                return StatusCode(500, new { success = false, message = "Failed to extract file ID" });
            }
        }

        /// <summary>
        /// Get image URL from file ID
        /// </summary>
        /// <param name="fileId">Unique file identifier</param>
        /// <returns>Generated image URL</returns>
        [HttpGet("url/{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUrlFromFileId([FromRoute] string fileId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                {
                    return BadRequest(new { success = false, message = "File ID is required" });
                }

                _logger.LogInformation("Generating URL for file ID: {FileId}", fileId);

                var imageUrl = await _imageService.GetUrlFromFileIdAsync(fileId);

                return Ok(new { success = true, imageUrl = imageUrl });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file ID");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating URL for file ID: {FileId}", fileId);
                return StatusCode(500, new { success = false, message = "Failed to generate image URL" });
            }
        }
    }
}
