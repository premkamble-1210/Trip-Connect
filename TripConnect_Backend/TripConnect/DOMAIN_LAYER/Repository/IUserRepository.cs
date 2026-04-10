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
    }
}
