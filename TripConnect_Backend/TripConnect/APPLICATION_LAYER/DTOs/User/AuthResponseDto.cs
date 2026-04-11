namespace APPLICATION_LAYER.DTOs.User
{
    /// <summary>
    /// Authentication Response DTO - Return JWT token
    /// </summary>
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserResponseDto User { get; set; }
    }
}
