using APPLICATION_LAYER.DTOs.JoinRequest;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Join Request Controller - Handles trip join requests
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class JoinRequestController : ControllerBase
    {
        private readonly IJoinRequestService _joinRequestService;
        private readonly ILogger<JoinRequestController> _logger;

        public JoinRequestController(IJoinRequestService joinRequestService, ILogger<JoinRequestController> logger)
        {
            _joinRequestService = joinRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Send join request for a trip
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendJoinRequest([FromBody] SendJoinRequestDto sendJoinRequestDto, [FromQuery] int userId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _joinRequestService.SendJoinRequestAsync(sendJoinRequestDto, userId);
                return CreatedAtAction(nameof(GetJoinRequestById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending join request");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get join request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJoinRequestById([FromRoute] int id)
        {
            try
            {
                var request = await _joinRequestService.GetJoinRequestByIdAsync(id);
                return Ok(request);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching join request");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get pending requests for a trip
        /// </summary>
        [HttpGet("trip/{tripId}/pending")]
        public async Task<IActionResult> GetPendingRequestsByTrip([FromRoute] int tripId)
        {
            try
            {
                var requests = await _joinRequestService.GetPendingRequestsByTripAsync(tripId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending requests");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get requests sent by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRequestsByUser([FromRoute] int userId)
        {
            try
            {
                var requests = await _joinRequestService.GetRequestsByUserAsync(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching requests by user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Accept join request
        /// </summary>
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptJoinRequest([FromRoute] int id, [FromQuery] int tripHostId)
        {
            try
            {
                var result = await _joinRequestService.AcceptJoinRequestAsync(id, tripHostId);
                return Ok(new { success = result, message = result ? "Join request accepted successfully" : "Join request acceptance failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting join request");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Reject join request
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectJoinRequest([FromRoute] int id, [FromQuery] int tripHostId)
        {
            try
            {
                var result = await _joinRequestService.RejectJoinRequestAsync(id, tripHostId);
                return Ok(new { success = result, message = result ? "Join request rejected successfully" : "Join request rejection failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting join request");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Cancel join request
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelJoinRequest([FromRoute] int id, [FromQuery] int userId)
        {
            try
            {
                var result = await _joinRequestService.CancelJoinRequestAsync(id, userId);
                return Ok(new { success = result, message = result ? "Join request cancelled successfully" : "Join request cancellation failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling join request");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if user already requested to join
        /// </summary>
        [HttpGet("check")]
        public async Task<IActionResult> HasUserRequested([FromQuery] int userId, [FromQuery] int tripId)
        {
            try
            {
                var hasRequested = await _joinRequestService.HasUserRequestedAsync(userId, tripId);
                return Ok(new { userId = userId, tripId = tripId, hasRequested = hasRequested });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user requested");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all requests for trip
        /// </summary>
        [HttpGet("trip/{tripId}")]
        public async Task<IActionResult> GetAllRequestsByTrip([FromRoute] int tripId)
        {
            try
            {
                var requests = await _joinRequestService.GetAllRequestsByTripAsync(tripId);
                return Ok(requests);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all requests for trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}
