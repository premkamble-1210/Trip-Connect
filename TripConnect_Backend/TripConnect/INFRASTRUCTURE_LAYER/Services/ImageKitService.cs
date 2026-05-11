using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Implementation of ImageKit service for image upload and management
    /// Uses ImageKit REST API directly via HttpClient for flexible integration
    /// Handles all ImageKit API interactions with error handling and logging
    /// </summary>
    public class ImageKitService : IImageKitService
    {
        private readonly ILogger<ImageKitService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _urlEndpoint;
        private readonly int _maxFileSizeMB;
        private readonly List<string> _allowedFormats;
        
        private const string UPLOAD_API_ENDPOINT = "https://upload.imagekit.io/api/v1/files/upload";
        private const string DELETE_API_ENDPOINT = "https://api.imagekit.io/v1/files";

        // Allowed image formats
        private static readonly List<string> DEFAULT_ALLOWED_FORMATS = new() { "jpg", "jpeg", "png", "webp" };
        private const int DEFAULT_MAX_FILE_SIZE_MB = 5;

        public ImageKitService(
            IConfiguration configuration,
            ILogger<ImageKitService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // Initialize ImageKit configuration
            _publicKey = _configuration["ImageKit:PublicKey"];
            _privateKey = _configuration["ImageKit:PrivateKey"];
            _urlEndpoint = _configuration["ImageKit:UrlEndpoint"];

            if (string.IsNullOrEmpty(_publicKey) || string.IsNullOrEmpty(_privateKey) || string.IsNullOrEmpty(_urlEndpoint))
            {
                throw new InvalidOperationException(
                    "ImageKit configuration is missing. Please configure 'ImageKit:PublicKey', " +
                    "'ImageKit:PrivateKey', and 'ImageKit:UrlEndpoint' in appsettings.json");
            }

            // Load image upload configuration
            _maxFileSizeMB = int.TryParse(_configuration["ImageUpload:MaxFileSizeInMB"], out var size) 
                ? size 
                : DEFAULT_MAX_FILE_SIZE_MB;

            var formatsConfig = _configuration.GetSection("ImageUpload:AllowedFormats").Get<List<string>>();
            _allowedFormats = formatsConfig ?? DEFAULT_ALLOWED_FORMATS;

            _logger.LogInformation("ImageKit service initialized successfully. Max file size: {MaxSize}MB, Allowed formats: {Formats}",
                _maxFileSizeMB, string.Join(", ", _allowedFormats));
        }

        /// <summary>
        /// Validates file before upload
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateFile(Stream fileStream, string fileName)
        {
            try
            {
                // Check file size
                if (fileStream.Length == 0)
                {
                    return (false, "File is empty");
                }

                var fileSizeInMB = fileStream.Length / (1024.0 * 1024.0);
                if (fileSizeInMB > _maxFileSizeMB)
                {
                    return (false, $"File size {fileSizeInMB:F2}MB exceeds maximum allowed size of {_maxFileSizeMB}MB");
                }

                // Check file extension
                var fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLower();
                if (!_allowedFormats.Contains(fileExtension))
                {
                    return (false, $"File format '{fileExtension}' is not allowed. Allowed formats: {string.Join(", ", _allowedFormats)}");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file validation");
                return (false, $"Validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sanitizes filename to prevent path traversal attacks
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            try
            {
                // Get only the file name, removing any directory path
                var sanitized = Path.GetFileName(fileName);

                // Remove any special characters except dots and hyphens
                sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-zA-Z0-9._-]", "_");

                // Ensure the filename is not empty
                if (string.IsNullOrWhiteSpace(sanitized))
                {
                    sanitized = $"file_{Guid.NewGuid()}".Substring(0, 20);
                }

                _logger.LogDebug("Sanitized filename from '{Original}' to '{Sanitized}'", fileName, sanitized);
                return sanitized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during filename sanitization");
                throw;
            }
        }

        /// <summary>
        /// Upload an image to ImageKit
        /// </summary>
        public async Task<ImageUploadResponse> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string folder = "uploads")
        {
            var originalFileName = fileName;
            try
            {
                _logger.LogInformation("Starting image upload: {FileName} to folder: {Folder}", fileName, folder);

                // Validate file
                var (isValid, validationError) = ValidateFile(imageStream, fileName);
                if (!isValid)
                {
                    _logger.LogWarning("File validation failed for {FileName}: {Error}", fileName, validationError);
                    return new ImageUploadResponse
                    {
                        Success = false,
                        ErrorMessage = validationError,
                        FileName = originalFileName
                    };
                }

                // Sanitize filename
                fileName = SanitizeFileName(fileName);

                // Reset stream position
                if (imageStream.CanSeek)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);
                }

                // Read file asynchronously
                var fileBytes = new byte[imageStream.Length];
                int bytesRead = await imageStream.ReadAsync(fileBytes, 0, fileBytes.Length);

                if (bytesRead == 0)
                {
                    _logger.LogWarning("Failed to read file bytes for: {FileName}", fileName);
                    return new ImageUploadResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to read file content",
                        FileName = originalFileName
                    };
                }

                // ===== ImageKit REST API Integration =====
                // POST https://upload.imagekit.io/api/v1/files/upload
                // Requires Basic Auth with private key
                
                // Create multipart form data for file upload
                using (var content = new MultipartFormDataContent())
                {
                    // Add file content - use ByteArrayContent with already-read bytes to avoid Content-Length issues
                    var fileContent = new ByteArrayContent(fileBytes, 0, bytesRead);
                    fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/octet-stream");
                    content.Add(fileContent, "file", fileName);

                    // Add fileName (required by ImageKit API)
                    content.Add(new StringContent(fileName), "fileName");

                    // Add folder parameter
                    content.Add(new StringContent(folder), "folder");
                    
                    // Add optional metadata
                    content.Add(new StringContent("true"), "useUniqueFileName");
                    content.Add(new StringContent("tripconnect,auto-uploaded"), "tags");

                    _logger.LogDebug("Sending file to ImageKit: {FileName} ({Bytes} bytes) to folder: {Folder}",
                        fileName, bytesRead, folder);

                    // Create request with Basic Auth
                    var request = new HttpRequestMessage(HttpMethod.Post, UPLOAD_API_ENDPOINT)
                    {
                        Content = content
                    };
                    
                    // Add Accept header
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Add Basic Authentication (privateKey:)
                    var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_privateKey}:"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                    try
                    {
                        var response = await _httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogDebug("ImageKit API response: {Response}", responseContent);

                        // Parse JSON response
                        using (JsonDocument doc = JsonDocument.Parse(responseContent))
                        {
                            var root = doc.RootElement;

                            // Check for error in response
                            if (root.TryGetProperty("error", out var errorElement))
                            {
                                var errorMessage = errorElement.GetProperty("message").GetString() ?? "Unknown error";
                                _logger.LogError("ImageKit upload failed for {FileName}: {Error}", fileName, errorMessage);
                                return new ImageUploadResponse
                                {
                                    Success = false,
                                    ErrorMessage = $"ImageKit API error: {errorMessage}",
                                    FileName = originalFileName
                                };
                            }

                            // Map response to our model
                            var uploadResponse = new ImageUploadResponse
                            {
                                FileId = root.GetProperty("fileId").GetString() ?? Guid.NewGuid().ToString(),
                                Url = root.GetProperty("url").GetString() ?? $"{_urlEndpoint}/{folder.TrimStart('/')}/{fileName}",
                                FilePath = root.GetProperty("filePath").GetString() ?? $"/{folder.TrimStart('/')}/{fileName}",
                                FileName = root.GetProperty("name").GetString() ?? fileName,
                                FileSize = root.TryGetProperty("size", out var sizeElem) ? sizeElem.GetInt64() : bytesRead,
                                Width = root.TryGetProperty("width", out var widthElem) ? widthElem.GetInt32() : 0,
                                Height = root.TryGetProperty("height", out var heightElem) ? heightElem.GetInt32() : 0,
                                UploadedAt = root.TryGetProperty("createdAt", out var dateElem) 
                                    ? DateTime.Parse(dateElem.GetString() ?? DateTime.UtcNow.ToString())
                                    : DateTime.UtcNow,
                                Success = true,
                                ErrorMessage = null
                            };

                            var fileSizeMB = uploadResponse.FileSize / (1024.0 * 1024.0);
                            _logger.LogInformation("Image upload successful via ImageKit: {FileName} (FileId: {FileId}, Size: {Size:F2}MB, URL: {Url})",
                                fileName, uploadResponse.FileId, fileSizeMB, uploadResponse.Url);
                            return uploadResponse;
                        }
                    }
                    catch (HttpRequestException hexc)
                    {
                        _logger.LogError(hexc, "ImageKit API request failed for {FileName}", fileName);
                        return new ImageUploadResponse
                        {
                            Success = false,
                            ErrorMessage = $"ImageKit API error: {hexc.Message}",
                            FileName = originalFileName
                        };
                    }
                }
                // =========================================
            }
            catch (InvalidOperationException iex)
            {
                // Configuration or validation errors
                _logger.LogError(iex, "Configuration error during image upload: {FileName}", originalFileName);
                return new ImageUploadResponse
                {
                    Success = false,
                    ErrorMessage = $"Configuration error: {iex.Message}",
                    FileName = originalFileName
                };
            }
            catch (IOException ioex)
            {
                // File I/O errors
                _logger.LogError(ioex, "I/O error during image upload: {FileName}", originalFileName);
                return new ImageUploadResponse
                {
                    Success = false,
                    ErrorMessage = "File access error occurred during upload",
                    FileName = originalFileName
                };
            }
            catch (Exception ex)
            {
                // Unexpected errors
                _logger.LogError(ex, "Unexpected exception during image upload: {FileName}", originalFileName);
                return new ImageUploadResponse
                {
                    Success = false,
                    ErrorMessage = $"Upload failed: {ex.GetType().Name} - {ex.Message}",
                    FileName = originalFileName
                };
            }
        }

        /// <summary>
        /// Delete an image from ImageKit by file ID
        /// </summary>
        public async Task<bool> DeleteImageAsync(string fileId)
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    _logger.LogWarning("Attempted to delete image with empty fileId");
                    return false;
                }

                _logger.LogInformation("Starting image deletion for fileId: {FileId}", fileId);

                // ===== ImageKit REST API Integration =====
                // DELETE https://api.imagekit.io/v1/files/{fileId}
                // Requires Basic Auth with private key
                
                var deleteUrl = $"{DELETE_API_ENDPOINT}/{fileId}";
                var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);

                // Add Basic Authentication (privateKey:)
                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_privateKey}:"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                try
                {
                    var response = await _httpClient.SendAsync(request);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("ImageKit deletion failed for fileId {FileId}: {StatusCode} - {Error}",
                            fileId, response.StatusCode, errorContent);
                        return false;
                    }

                    _logger.LogInformation("Image deleted successfully via ImageKit: {FileId}", fileId);
                    return true;
                }
                catch (HttpRequestException hexc)
                {
                    _logger.LogError(hexc, "ImageKit API request failed for deletion: {FileId}", fileId);
                    return false;
                }
                // =========================================
            }
            catch (HttpRequestException hexc)
            {
                // Network/API errors
                _logger.LogError(hexc, "Network error during image deletion: {FileId}", fileId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception during image deletion: {FileId}", fileId);
                return false;
            }
        }

        /// <summary>
        /// Generate an optimized image URL with optional transformations
        /// </summary>
        public async Task<string> GetImageUrlAsync(
            string filePath,
            int? width = null,
            int? height = null,
            int? quality = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogWarning("Attempted to get URL for empty filePath");
                    return string.Empty;
                }

                // ===== ImageKit URL Generation =====
                // ImageKit uses standard URL format with transformation parameters
                // Format: https://ik.imagekit.io/your-url-endpoint/path/to/image.jpg?tr=w-400:h-300:q-80
                // No API call required for URL generation
                
                var url = $"{_urlEndpoint}{(filePath.StartsWith("/") ? "" : "/")}{filePath}";

                // Build transformation query parameters in ImageKit format
                if (width.HasValue || height.HasValue || quality.HasValue)
                {
                    var transformations = new List<string>();
                    
                    if (width.HasValue && width > 0)
                        transformations.Add($"w-{width}");
                    
                    if (height.HasValue && height > 0)
                        transformations.Add($"h-{height}");
                    
                    if (quality.HasValue && quality > 0)
                    {
                        var clampedQuality = Math.Clamp(quality.Value, 1, 100);
                        transformations.Add($"q-{clampedQuality}");
                    }

                    if (transformations.Count > 0)
                    {
                        url += "?tr=" + string.Join(":", transformations);
                    }
                }

                _logger.LogInformation("Generated image URL for: {FilePath} with transformations W:{Width} H:{Height} Q:{Quality}, URL: {GeneratedUrl}",
                    filePath, width, height, quality, url);
                return url;
                // ====================================
            }
            catch (ArgumentException argex)
            {
                // Invalid argument errors
                _logger.LogError(argex, "Invalid argument while generating image URL: {FilePath}", filePath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception while generating image URL: {FilePath}", filePath);
                return string.Empty;
            }
        }
    }
}
