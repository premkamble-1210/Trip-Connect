using APPLICATION_LAYER.DTOs.Auth;
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
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper, ITokenService tokenService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _tokenService = tokenService;
            _emailService = emailService;
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

                // Generate salt and hash password
                var passwordSalt = GenerateSalt();
                var passwordHash = HashPassword(createUserDto.Password, passwordSalt);

                // Generate refresh token
                var refreshToken = _tokenService.GenerateRefreshToken();

                var newUser = new User
                {
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    Username = createUserDto.Username,
                    Phone = createUserDto.Phone,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    PhoneVerified = false,
                    IdVerified = false,
                    Rating = 0.0,
                    CreatedAt = DateTime.UtcNow,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
                    LastLoginAt = DateTime.UtcNow,
                    IsTokenBlacklisted = false
                };

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"User registered successfully with ID: {newUser.Id}");

                // Generate JWT token
                var accessTokenDto = _tokenService.GenerateAccessToken(newUser);
                accessTokenDto.RefreshToken = refreshToken;

                var userDto = _mapper.Map<UserResponseDto>(newUser);
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = accessTokenDto,
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
                if (!VerifyPassword(loginUserDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.Warning($"Login failed - Invalid password for user: {user.Id}");
                    throw new InvalidOperationException("Invalid credentials");
                }

                // Generate new refresh token
                var refreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                user.LastLoginAt = DateTime.UtcNow;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"User logged in successfully: {user.Id}");

                // Generate JWT token
                var accessTokenDto = _tokenService.GenerateAccessToken(user);
                accessTokenDto.RefreshToken = refreshToken;

                var userDto = _mapper.Map<UserResponseDto>(user);
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = accessTokenDto,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task LogoutAsync(int userId)
        {
            try
            {
                _logger.Information($"Logout request for user: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for logout: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                // Clear refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.IsTokenBlacklisted = true;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"User logged out successfully: {userId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Logout error: {ex.Message}");
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

        public async Task RequestEmailVerificationAsync(int userId)
        {
            try
            {
                _logger.Information($"Email verification requested for user: {userId}");
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.Warning($"User not found for email verification: {userId}");
                    throw new InvalidOperationException("User not found");
                }

                if (user.EmailVerified)
                {
                    _logger.Warning($"Email already verified for user: {userId}");
                    throw new InvalidOperationException("Email is already verified");
                }

                // Generate 32-byte Base64 token — same pattern as refresh token
                var tokenBytes = new byte[32];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(tokenBytes);
                }
                var token = Convert.ToBase64String(tokenBytes);

                user.EmailVerificationToken = token;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // URL-encode because Base64 contains +, / and = characters
                var encodedToken = Uri.EscapeDataString(token);
                var verificationUrl = $"https://localhost:7142/api/user/verify-email?token={encodedToken}";

                await _emailService.SendEmailVerificationAsync(user.Email, user.Name, verificationUrl);

                _logger.Information($"Verification email dispatched for user: {userId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error requesting email verification for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                _logger.Information("Email verification attempt");

                var user = await _unitOfWork.Users.GetByEmailVerificationTokenAsync(token);

                if (user == null)
                {
                    _logger.Warning("Email verification failed — token not found");
                    throw new InvalidOperationException("Invalid verification token");
                }

                if (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                {
                    _logger.Warning($"Email verification token expired for user: {user.Id}");
                    throw new InvalidOperationException("Verification token has expired");
                }

                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Email verified successfully for user: {user.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error verifying email: {ex.Message}");
                throw;
            }
        }

        // Helper Methods
        private string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var hmac = new HMACSHA512(saltBytes))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private bool VerifyPassword(string password, string storedHash, string salt)
        {
            var hashOfInput = HashPassword(password, salt);
            return hashOfInput == storedHash;
        }
    }
}
