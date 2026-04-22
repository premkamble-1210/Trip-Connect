namespace APPLICATION_LAYER.DTOs.User
{
    /// <summary>
    /// User Response DTO - Return user info
    /// </summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public double Rating { get; set; }
        public bool PhoneVerified { get; set; }
        public bool IdVerified { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
