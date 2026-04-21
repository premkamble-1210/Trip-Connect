using APPLICATION_LAYER.DTOs.Auth;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Interface for managing refresh token operations
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Refreshes an expired access token using a valid refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token from the user</param>
        /// <returns>New JWT token response with updated access token</returns>
        Task<JwtTokenResponseDto> RefreshAccessTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes/blacklists a refresh token (used during logout)
        /// </summary>
        /// <param name="userId">ID of the user logging out</param>
        Task RevokeTokenAsync(int userId);

        /// <summary>
        /// Validates if a refresh token is still valid for a user
        /// </summary>
        /// <param name="token">The refresh token to validate</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>True if token is valid, false otherwise</returns>
        Task<bool> IsTokenValidAsync(string token, int userId);
    }
}
