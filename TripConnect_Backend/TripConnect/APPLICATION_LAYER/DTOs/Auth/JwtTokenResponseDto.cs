namespace APPLICATION_LAYER.DTOs.Auth
{
    /// <summary>
    /// JWT Token Response DTO - Contains access token and refresh token details
    /// </summary>
    public class JwtTokenResponseDto
    {
        /// <summary>
        /// The JWT access token (used for API requests)
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token (used to get a new access token)
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Token type (typically "Bearer")
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Access token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Timestamp when the token was issued
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// Timestamp when the token will expire
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
