namespace APPLICATION_LAYER.Models
{
    /// <summary>
    /// JWT Settings configuration model
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key for signing JWT tokens (minimum 32 characters for HS256)
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Issuer of the JWT token (e.g., "TripConnectAPI")
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Audience for which the JWT token is intended (e.g., "TripConnectApp")
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Access token expiration time in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; }

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; }
    }
}
