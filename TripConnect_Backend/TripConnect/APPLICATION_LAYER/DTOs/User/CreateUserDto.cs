namespace APPLICATION_LAYER.DTOs.User
{
    /// <summary>
    /// User Registration/Create DTO
    /// </summary>
    public class CreateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
