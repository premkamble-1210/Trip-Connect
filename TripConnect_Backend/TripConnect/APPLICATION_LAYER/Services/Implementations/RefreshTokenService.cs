using APPLICATION_LAYER.DTOs.Auth;
using APPLICATION_LAYER.Services.Interfaces;
using DOMAIN_LAYER.Repository;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    /// <summary>
    /// Service for managing refresh token operations
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        private readonly HashSet<string> _tokenBlacklist = new(); // In-memory blacklist (use Redis in production)

        public RefreshTokenService(IUnitOfWork unitOfWork, ITokenService tokenService, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token
        /// </summary>
        public async Task<JwtTokenResponseDto> RefreshAccessTokenAsync(string refreshToken)
        {
            try
            {
                _logger.Information("Refresh token request received");

                // Check if token is blacklisted
                if (_tokenBlacklist.Contains(refreshToken))
                {
                    _logger.Warning("Attempted to use blacklisted refresh token");
                    throw new InvalidOperationException("Refresh token has been revoked");
                }

                // Find user with matching refresh token
                var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);

                if (user == null)
                {
                    _logger.Warning("Refresh token not found or user not found");
                    throw new InvalidOperationException("Invalid refresh token");
                }

                // Check if refresh token is expired
                if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                {
                    _logger.Warning($"Refresh token expired for user: {user.Id}");
                    throw new InvalidOperationException("Refresh token has expired");
                }

                // Generate new access token
                var newAccessToken = _tokenService.GenerateAccessToken(user);
                newAccessToken.RefreshToken = refreshToken;

                _logger.Information($"Access token refreshed for user: {user.Id}");
                return newAccessToken;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error refreshing token: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Revokes/blacklists a refresh token (used during logout)
        /// </summary>
        public async Task RevokeTokenAsync(int userId)
        {
            try
            {
                _logger.Information($"Revoking refresh token for user: {userId}");

                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for token revocation: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                // Add token to blacklist
                if (!string.IsNullOrEmpty(user.RefreshToken))
                {
                    _tokenBlacklist.Add(user.RefreshToken);
                }

                // Clear refresh token from database
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.IsTokenBlacklisted = true;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Refresh token revoked for user: {userId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error revoking token for user {userId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validates if a refresh token is still valid for a user
        /// </summary>
        public async Task<bool> IsTokenValidAsync(string token, int userId)
        {
            try
            {
                // Check if blacklisted
                if (_tokenBlacklist.Contains(token))
                {
                    return false;
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null || user.IsTokenBlacklisted)
                {
                    return false;
                }

                // Check if token matches and hasn't expired
                return user.RefreshToken == token && user.RefreshTokenExpiryTime.HasValue && user.RefreshTokenExpiryTime > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error validating token: {ex.Message}");
                return false;
            }
        }
    }
}
