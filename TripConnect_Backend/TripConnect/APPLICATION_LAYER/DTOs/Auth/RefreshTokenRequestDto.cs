namespace APPLICATION_LAYER.DTOs.Auth
{
    /// <summary>
    /// Refresh Token Request DTO - Used to request a new access token
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// The refresh token received during login or registration
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
