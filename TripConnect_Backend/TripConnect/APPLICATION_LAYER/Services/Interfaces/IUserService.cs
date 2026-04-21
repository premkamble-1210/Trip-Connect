using APPLICATION_LAYER.DTOs.User;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// User Service Interface - Handles user authentication and profile management
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(CreateUserDto dto);

        /// <summary>
        /// Login user with username and password
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);

        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<UserResponseDto> GetUserByIdAsync(int userId);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<UserResponseDto> GetUserByEmailAsync(string email);

        /// <summary>
        /// Update user profile
        /// </summary>
        Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserDto dto);

        /// <summary>
        /// Get user rating/reputation
        /// </summary>
        Task<double> GetUserRatingAsync(int userId);

        /// <summary>
        /// Get all users (admin)
        /// </summary>
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

        /// <summary>
        /// Verify user phone number
        /// </summary>
        Task<bool> VerifyPhoneAsync(int userId);

        /// <summary>
        /// Verify user ID
        /// </summary>
        Task<bool> VerifyIdAsync(int userId);

        /// <summary>
        /// Check if username exists
        /// </summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Check if email exists
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        Task LogoutAsync(int userId);
    }
}
