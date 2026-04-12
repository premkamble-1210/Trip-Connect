using APPLICATION_LAYER.DTOs.Rating;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Rating Controller - Handles user ratings and reviews
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingService ratingService, ILogger<RatingController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        /// <summary>
        /// Create rating for user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto createRatingDto, [FromQuery] int userId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _ratingService.CreateRatingAsync(createRatingDto, userId);
                return CreatedAtAction(nameof(GetRatingById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get rating by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRatingById([FromRoute] int id)
        {
            try
            {
                var rating = await _ratingService.GetRatingByIdAsync(id);
                return Ok(rating);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rating");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all ratings for user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRatingsForUser([FromRoute] int userId)
        {
            try
            {
                var ratings = await _ratingService.GetRatingsForUserAsync(userId);
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ratings for user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get average rating for user
        /// </summary>
        [HttpGet("user/{userId}/average")]
        public async Task<IActionResult> GetAverageRating([FromRoute] int userId)
        {
            try
            {
                var average = await _ratingService.GetAverageRatingAsync(userId);
                return Ok(new { userId = userId, averageRating = average });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching average rating");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get ratings given by user
        /// </summary>
        [HttpGet("user/{userId}/given")]
        public async Task<IActionResult> GetRatingsGivenByUser([FromRoute] int userId)
        {
            try
            {
                var ratings = await _ratingService.GetRatingsGivenByUserAsync(userId);
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ratings given by user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get ratings for trip
        /// </summary>
        [HttpGet("trip/{tripId}")]
        public async Task<IActionResult> GetRatingsForTrip([FromRoute] int tripId)
        {
            try
            {
                var ratings = await _ratingService.GetRatingsForTripAsync(tripId);
                return Ok(ratings);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ratings for trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if user already rated another user in trip
        /// </summary>
        [HttpGet("check")]
        public async Task<IActionResult> HasUserRated([FromQuery] int tripId, [FromQuery] int ratedBy, [FromQuery] int ratedUserId)
        {
            try
            {
                var hasRated = await _ratingService.HasUserRatedAsync(tripId, ratedBy, ratedUserId);
                return Ok(new { tripId = tripId, ratedBy = ratedBy, ratedUserId = ratedUserId, hasRated = hasRated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user rated");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update rating
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRating([FromRoute] int id, [FromBody] CreateRatingDto createRatingDto, [FromQuery] int userId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _ratingService.UpdateRatingAsync(id, createRatingDto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete rating
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating([FromRoute] int id, [FromQuery] int userId)
        {
            try
            {
                var result = await _ratingService.DeleteRatingAsync(id, userId);
                return Ok(new { success = result, message = result ? "Rating deleted successfully" : "Rating deletion failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get ratings count for user
        /// </summary>
        [HttpGet("user/{userId}/count")]
        public async Task<IActionResult> GetRatingsCount([FromRoute] int userId)
        {
            try
            {
                var count = await _ratingService.GetRatingsCountAsync(userId);
                return Ok(new { userId = userId, ratingsCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ratings count");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}
