namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// User Repository Interface - Specific operations for User entity
    /// </summary>
    public interface IUserRepository : IRepository<Entity.User.User>
    {
        /// <summary>
        /// Get user by email
        /// </summary>
        Task<Entity.User.User> GetByEmailAsync(string email);

        /// <summary>
        /// Get user by phone
        /// </summary>
        Task<Entity.User.User> GetByPhoneAsync(string phone);

        /// <summary>
        /// Get all verified users
        /// </summary>
        Task<IEnumerable<Entity.User.User>> GetVerifiedUsersAsync();

        /// <summary>
        /// Update user verification status
        /// </summary>
        Task UpdateVerificationStatusAsync(int userId, bool phoneVerified, bool idVerified);

        /// <summary>
        /// Update user rating
        /// </summary>
        Task UpdateRatingAsync(int userId, double rating);

        /// <summary>
        /// Get user by username (for authentication)
        /// </summary>
        Task<Entity.User.User> GetByUsernameAsync(string username);

        /// <summary>
        /// Check if email already exists
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Check if phone already exists
        /// </summary>
        Task<bool> PhoneExistsAsync(string phone);

        /// <summary>
        /// Check if username already exists
        /// </summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Update user password hash and salt
        /// </summary>
        Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt);

        /// <summary>
        /// Get user with password for authentication
        /// </summary>
        Task<Entity.User.User> GetUserForAuthenticationAsync(string username);

        /// <summary>
        /// Get user by refresh token
        /// </summary>
        Task<Entity.User.User> GetByRefreshTokenAsync(string refreshToken);
    }
}
