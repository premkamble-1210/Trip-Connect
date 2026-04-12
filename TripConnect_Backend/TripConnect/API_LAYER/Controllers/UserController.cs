using APPLICATION_LAYER.DTOs.User;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="createUserDto">User registration details</param>
        /// <returns>Auth response with user info</returns>
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
        /// <returns>Auth response with user info</returns>
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
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
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
        /// Update user profile
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">Updated user details</param>
        /// <returns>Updated user info</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
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
    }
}
