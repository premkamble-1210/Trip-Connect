using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.User;
using Microsoft.EntityFrameworkCore;

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// User Repository Implementation - Specific operations for User entity
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Get user by phone
        /// </summary>
        public async Task<User> GetByPhoneAsync(string phone)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone);
        }

        /// <summary>
        /// Get user by username (for authentication)
        /// </summary>
        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Get all verified users
        /// </summary>
        public async Task<IEnumerable<User>> GetVerifiedUsersAsync()
        {
            return await _dbSet.Where(u => u.PhoneVerified && u.IdVerified).ToListAsync();
        }

        /// <summary>
        /// Update user verification status
        /// </summary>
        public async Task UpdateVerificationStatusAsync(int userId, bool phoneVerified, bool idVerified)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.PhoneVerified = phoneVerified;
                user.IdVerified = idVerified;
                await UpdateAsync(user);
            }
        }

        /// <summary>
        /// Update user rating
        /// </summary>
        public async Task UpdateRatingAsync(int userId, double rating)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.Rating = rating;
                await UpdateAsync(user);
            }
        }

        /// <summary>
        /// Check if email already exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Check if phone already exists
        /// </summary>
        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _dbSet.AnyAsync(u => u.Phone == phone);
        }

        /// <summary>
        /// Check if username already exists
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username);
        }

        /// <summary>
        /// Update user password hash and salt
        /// </summary>
        public async Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                await UpdateAsync(user);
            }
        }

        /// <summary>
        /// Get user with password for authentication
        /// </summary>
        public async Task<User> GetUserForAuthenticationAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Get user by refresh token
        /// </summary>
        public async Task<User> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }
    }
}
