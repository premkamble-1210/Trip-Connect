using APPLICATION_LAYER.DTOs.Auth;

namespace APPLICATION_LAYER.DTOs.User
{
    /// <summary>
    /// Authentication Response DTO - Return JWT tokens and user information
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Indicates if the authentication was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Authentication response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// JWT token details (access token, refresh token, expiration)
        /// </summary>
        public JwtTokenResponseDto Token { get; set; }

        /// <summary>
        /// Authenticated user information
        /// </summary>
        public UserResponseDto User { get; set; }
    }
}
