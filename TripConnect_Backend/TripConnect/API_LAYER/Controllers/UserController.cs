using APPLICATION_LAYER.DTOs.Auth;
using APPLICATION_LAYER.DTOs.User;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// User Controller - Handles user authentication and profile management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IRefreshTokenService refreshTokenService, IRecommendationService recommendationService, ILogger<UserController> logger)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _recommendationService = recommendationService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="createUserDto">User registration details</param>
        /// <returns>Auth response with JWT tokens and user info</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.RegisterAsync(createUserDto);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginUserDto">Username and password</param>
        /// <returns>Auth response with JWT tokens and user info</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.LoginAsync(loginUserDto);
                
                if (!result.Success)
                    return Unauthorized(result);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="refreshTokenRequest">Refresh token from user</param>
        /// <returns>New JWT access token</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _refreshTokenService.RefreshAccessTokenAsync(refreshTokenRequest.RefreshToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh error");
                return StatusCode(500, new { success = false, message = "An error occurred during token refresh" });
            }
        }

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user claim" });
                }

                await _userService.LogoutAsync(userId);
                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return StatusCode(500, new { success = false, message = "An error occurred during logout" });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User details</returns>
        [HttpGet("email/{email}")]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail([FromRoute] string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update user profile (authenticated user only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">Updated user details</param>
        /// <returns>Updated user info</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                // Verify user is updating their own profile
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var claimUserId) || claimUserId != id)
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.UpdateUserAsync(id, updateUserDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get user rating
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User rating score</returns>
        [HttpGet("{id}/rating")]
        public async Task<IActionResult> GetUserRating([FromRoute] int id)
        {
            try
            {
                var rating = await _userService.GetUserRatingAsync(id);
                return Ok(new { userId = id, rating = rating });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user rating");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Verify phone number
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Verification result</returns>
        [HttpPost("{id}/verify-phone")]
        [Authorize]
        public async Task<IActionResult> VerifyPhone([FromRoute] int id)
        {
            try
            {
                var result = await _userService.VerifyPhoneAsync(id);
                return Ok(new { success = result, message = result ? "Phone verified successfully" : "Phone verification failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying phone");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Verify ID document
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Verification result</returns>
        [HttpPost("{id}/verify-id")]
        [Authorize]
        public async Task<IActionResult> VerifyId([FromRoute] int id)
        {
            try
            {
                var result = await _userService.VerifyIdAsync(id);
                return Ok(new { success = result, message = result ? "ID verified successfully" : "ID verification failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying ID");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Boolean indicating if email exists</returns>
        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> EmailExists([FromRoute] string email)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                return Ok(new { email = email, exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Boolean indicating if username exists</returns>
        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> UsernameExists([FromRoute] string username)
        {
            try
            {
                var exists = await _userService.UsernameExistsAsync(username);
                return Ok(new { username = username, exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Request email verification — generates token and sends verification email
        /// </summary>
        /// <param name="id">User ID (must match authenticated user)</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/request-email-verification")]
        [Authorize]
        public async Task<IActionResult> RequestEmailVerification([FromRoute] int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var claimUserId) || claimUserId != id)
                {
                    return Forbid();
                }

                await _userService.RequestEmailVerificationAsync(id);
                return Ok(new { success = true, message = "Verification email sent. Please check your inbox." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email");
                return StatusCode(500, new { success = false, message = "Failed to send verification email" });
            }
        }

        /// <summary>
        /// Verify email address using the token from the verification link
        /// </summary>
        /// <param name="token">Email verification token (from query string)</param>
        /// <returns>Redirect to frontend profile page</returns>
        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                await _userService.VerifyEmailAsync(token);
                return Redirect("http://localhost:4200/profile?emailVerified=true");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Redirect($"http://localhost:4200/profile?emailVerified=false&reason={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return Redirect("http://localhost:4200/profile?emailVerified=false&reason=server_error");
            }
        }

        /// <summary>
        /// Request phone OTP — generates a 6-digit OTP and sends it via SMS (max 3 per user)
        /// </summary>
        /// <param name="id">User ID (must match authenticated user)</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/request-phone-verification")]
        [Authorize]
        public async Task<IActionResult> RequestPhoneVerification([FromRoute] int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var claimUserId) || claimUserId != id)
                {
                    return Forbid();
                }

                await _userService.RequestPhoneVerificationAsync(id);
                return Ok(new { success = true, message = "OTP sent to your registered phone number." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending phone OTP");
                return StatusCode(500, new { success = false, message = "Failed to send OTP" });
            }
        }

        /// <summary>
        /// Verify phone OTP submitted by user
        /// </summary>
        /// <param name="id">User ID (must match authenticated user)</param>
        /// <param name="dto">OTP code submitted by user</param>
        /// <returns>Verification result</returns>
        [HttpPost("{id}/verify-phone-otp")]
        [Authorize]
        public async Task<IActionResult> VerifyPhoneOtp([FromRoute] int id, [FromBody] VerifyPhoneOtpDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var claimUserId) || claimUserId != id)
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.VerifyPhoneOtpAsync(id, dto.Otp);
                return Ok(new { success = result, message = "Phone verified successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying phone OTP");
                return StatusCode(500, new { success = false, message = "An error occurred during verification" });
            }
        }

        /// <summary>
        /// Get personalized trip recommendations (self-only)
        /// </summary>
        /// <param name="id">User ID (must match authenticated user)</param>
        /// <returns>Ranked list of recommended trips</returns>
        [HttpGet("{id}/recommendations")]
        [Authorize]
        public async Task<IActionResult> GetRecommendations([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation($"Fetching recommendations for user: {id}");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var claimUserId) || claimUserId != id)
                {
                    return Forbid();
                }

                var recommendations = await _recommendationService.GetRecommendationsAsync(id);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recommendations");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching recommendations" });
            }
        }
    }
}
