using APPLICATION_LAYER.DTOs.Auth;
using DOMAIN_LAYER.Entity.User;
using System.Security.Claims;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Interface for JWT token generation and validation
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the given user
        /// </summary>
        /// <param name="user">User entity to generate token for</param>
        /// <returns>JWT token response containing access token details</returns>
        JwtTokenResponseDto GenerateAccessToken(User user);

        /// <summary>
        /// Generates a refresh token (random secure string)
        /// </summary>
        /// <returns>A cryptographically secure refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates and extracts claims from an expired JWT token
        /// </summary>
        /// <param name="token">The expired JWT token</param>
        /// <returns>Claims principal containing user claims</returns>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
