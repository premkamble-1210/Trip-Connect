using APPLICATION_LAYER.DTOs.User;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Entity.User;
using DOMAIN_LAYER.Repository;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(CreateUserDto createUserDto)
        {
            try
            {
                _logger.Information($"Registration attempt for email: {createUserDto.Email}");

                // Check if user already exists
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(createUserDto.Email);
                if (existingEmail != null)
                {
                    _logger.Warning($"Registration failed - Email already exists: {createUserDto.Email}");
                    throw new InvalidOperationException("Email already registered");
                }

                // Check if username exists
                var usernameExists = await _unitOfWork.Users.UsernameExistsAsync(createUserDto.Username);
                if (usernameExists)
                {
                    _logger.Warning($"Registration failed - Username already exists: {createUserDto.Username}");
                    throw new InvalidOperationException("Username already taken");
                }

                // Hash password
                var passwordHash = HashPassword(createUserDto.Password);

                var newUser = new User
                {
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    Username = createUserDto.Username,
                    Phone = createUserDto.Phone,
                    PasswordHash = passwordHash,
                    PasswordSalt = GenerateSalt(),
                    PhoneVerified = false,
                    IdVerified = false,
                    Rating = 0.0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"User registered successfully with ID: {newUser.Id}");

                var userDto = _mapper.Map<UserResponseDto>(newUser);
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Registration error: {ex.Message}");
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto loginUserDto)
        {
            try
            {
                _logger.Information($"Login attempt for username: {loginUserDto.Username}");

                var user = await _unitOfWork.Users.GetByUsernameAsync(loginUserDto.Username);

                if (user == null)
                {
                    _logger.Warning($"Login failed - User not found: {loginUserDto.Username}");
                    throw new InvalidOperationException("Invalid credentials");
                }

                // Verify password
                if (!VerifyPassword(loginUserDto.Password, user.PasswordHash))
                {
                    _logger.Warning($"Login failed - Invalid password for user: {user.Id}");
                    throw new InvalidOperationException("Invalid credentials");
                }

                _logger.Information($"User logged in successfully: {user.Id}");

                var userDto = _mapper.Map<UserResponseDto>(user);
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching user: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                return _mapper.Map<UserResponseDto>(user);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDto> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.Information($"Fetching user by email: {email}");
                var user = await _unitOfWork.Users.GetByEmailAsync(email);

                if (user == null)
                {
                    _logger.Warning($"User not found with email: {email}");
                    throw new InvalidOperationException("User not found");
                }

                return _mapper.Map<UserResponseDto>(user);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching user by email {email}: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                _logger.Information($"Updating user: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for update: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                // Map DTO to user entity
                _mapper.Map(updateUserDto, user);

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"User updated successfully: {userId}");
                return _mapper.Map<UserResponseDto>(user);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<double> GetUserRatingAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching user rating: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for rating: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                return user.Rating;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching user rating {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            try
            {
                _logger.Information("Fetching all users");
                var users = await _unitOfWork.Users.GetAllAsync();
                return _mapper.Map<IEnumerable<UserResponseDto>>(users);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching all users: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyPhoneAsync(int userId)
        {
            try
            {
                _logger.Information($"Verifying phone for user: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for phone verification: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                user.PhoneVerified = true;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Phone verified successfully for user: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error verifying phone for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyIdAsync(int userId)
        {
            try
            {
                _logger.Information($"Verifying ID for user: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for ID verification: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                user.IdVerified = true;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"ID verified successfully for user: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error verifying ID for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking email existence: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                return await _unitOfWork.Users.UsernameExistsAsync(username);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking username existence: {ex.Message}");
                throw;
            }
        }

        // Helper Methods
        private string HashPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[16];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == storedHash;
        }
    }
}
