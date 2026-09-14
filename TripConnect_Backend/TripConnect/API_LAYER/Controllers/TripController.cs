using APPLICATION_LAYER.DTOs.Trip;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Trip Controller - Handles trip management and operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ILogger<TripController> _logger;

        public TripController(ITripService tripService, ILogger<TripController> logger)
        {
            _tripService = tripService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new trip (authenticated users only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto createTripDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Extract userId from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user claim" });
                }

                var result = await _tripService.CreateTripAsync(createTripDto, userId);
                return CreatedAtAction(nameof(GetTripById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get trip by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById([FromRoute] int id)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                return Ok(trip);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all trips with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTrips([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var trips = await _tripService.GetAllTripsAsync(pageNumber, pageSize);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all trips");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get trips by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetTripsByStatus([FromRoute] string status)
        {
            try
            {
                var trips = await _tripService.GetTripsByStatusAsync(status);
                return Ok(trips);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trips by status");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get upcoming trips
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingTrips()
        {
            try
            {
                var trips = await _tripService.GetUpcomingTripsAsync();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming trips");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Search trips by location http://localhost:5126/api/trip/search?location=g&maxBudget=2500
// Request Method
// GET
        /// </summary>
        [HttpGet("search/location/{location}")]
        public async Task<IActionResult> SearchTripsByLocation([FromRoute] string location)
        {
            try
            {
                var trips = await _tripService.SearchTripsByLocationAsync(location);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching trips by location");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Search trips by multiple criteria
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchTrips([FromQuery] string location, [FromQuery] DateTime? startDate, [FromQuery] decimal? minBudget, [FromQuery] decimal? maxBudget, [FromQuery] string travelType, [FromQuery] DateTime? endDate)
        {
            try
            {
                var trips = await _tripService.SearchTripsAsync(location, startDate, minBudget, maxBudget, travelType, endDate);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching trips");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get trips created by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTripsCreatedByUser([FromRoute] int userId)
        {
            try
            {
                var trips = await _tripService.GetTripsCreatedByUserAsync(userId);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trips created by user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update trip (authenticated users only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTrip([FromRoute] int id, [FromBody] UpdateTripDto updateTripDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Extract userId from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user claim" });
                }

                var result = await _tripService.UpdateTripAsync(id, updateTripDto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Cancel trip (authenticated users only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelTrip([FromRoute] int id)
        {
            try
            {
                // Extract userId from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user claim" });
                }

                var result = await _tripService.CancelTripAsync(id, userId);
                return Ok(new { success = result, message = result ? "Trip cancelled successfully" : "Trip cancellation failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get trips user is member of
        /// </summary>
        [HttpGet("member/{userId}")]
        public async Task<IActionResult> GetUserTrips([FromRoute] int userId)
        {
            try
            {
                var trips = await _tripService.GetUserTripsAsync(userId);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trips for user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get members of a trip
        /// </summary>
        [HttpGet("{tripId}/members")]
        public async Task<IActionResult> GetTripMembers([FromRoute] int tripId)
        {
            try
            {
                var members = await _tripService.GetTripMembersAsync(tripId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trip members");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}
