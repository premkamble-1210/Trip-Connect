using APPLICATION_LAYER.DTOs.Chat;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Chat Controller - Handles real-time messaging
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// Send message to trip chat
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto sendMessageDto, [FromQuery] int userId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for SendMessage");
                    return BadRequest(new { success = false, message = "Invalid message data" });
                }

                var chatMessageDto = await _chatService.SendMessageAsync(sendMessageDto, userId);
                _logger.LogInformation($"Message sent successfully for trip {sendMessageDto.TripId} by user {userId}");
                return CreatedAtAction(nameof(GetMessagesByTrip), new { tripId = sendMessageDto.TripId }, chatMessageDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation in SendMessage: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error sending message" });
            }
        }

        /// <summary>
        /// Get messages for trip
        /// </summary>
        [HttpGet("trip/{tripId}")]
        public async Task<IActionResult> GetMessagesByTrip([FromRoute] int tripId)
        {
            try
            {
                var messages = await _chatService.GetMessagesByTripAsync(tripId);
                _logger.LogInformation($"Retrieved messages for trip {tripId}");
                return Ok(messages);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Trip not found for GetMessagesByTrip: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error retrieving messages" });
            }
        }

        /// <summary>
        /// Get messages for trip with pagination
        /// </summary>
        [HttpGet("trip/{tripId}/paginated")]
        public async Task<IActionResult> GetMessagesByTripPaginated([FromRoute] int tripId, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                var messages = await _chatService.GetMessagesByTripPaginatedAsync(tripId, pageNumber, pageSize);
                _logger.LogInformation($"Retrieved paginated messages for trip {tripId}, page {pageNumber}, size {pageSize}");
                return Ok(messages);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Trip not found for GetMessagesByTripPaginated: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving paginated messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error retrieving paginated messages" });
            }
        }

        /// <summary>
        /// Get latest messages from trip
        /// </summary>
        [HttpGet("trip/{tripId}/latest")]
        public async Task<IActionResult> GetLatestMessages([FromRoute] int tripId, [FromQuery] int count)
        {
            try
            {
                var messages = await _chatService.GetLatestMessagesAsync(tripId, count);
                _logger.LogInformation($"Retrieved latest {count} messages for trip {tripId}");
                return Ok(messages);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Trip not found for GetLatestMessages: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving latest messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error retrieving latest messages" });
            }
        }

        /// <summary>
        /// Get messages sent by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMessagesBySender([FromRoute] int userId)
        {
            try
            {
                var messages = await _chatService.GetMessagesBySenderAsync(userId);
                _logger.LogInformation($"Retrieved messages sent by user {userId}");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving messages by sender: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error retrieving messages" });
            }
        }

        /// <summary>
        /// Search messages in trip
        /// </summary>
        [HttpGet("trip/{tripId}/search")]
        public async Task<IActionResult> SearchMessages([FromRoute] int tripId, [FromQuery] string searchText)
        {
            try
            {
                var messages = await _chatService.SearchMessagesAsync(tripId, searchText);
                _logger.LogInformation($"Searched messages in trip {tripId} with text: {searchText}");
                return Ok(messages);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Trip not found for SearchMessages: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error searching messages" });
            }
        }

        /// <summary>
        /// Get message count for trip
        /// </summary>
        [HttpGet("trip/{tripId}/count")]
        public async Task<IActionResult> GetMessageCount([FromRoute] int tripId)
        {
            try
            {
                var count = await _chatService.GetMessageCountAsync(tripId);
                _logger.LogInformation($"Retrieved message count for trip {tripId}");
                return Ok(new { tripId, messageCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving message count: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error retrieving message count" });
            }
        }

        /// <summary>
        /// Delete message
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage([FromRoute] int id, [FromQuery] int userId)
        {
            try
            {
                await _chatService.DeleteMessageAsync(id, userId);
                _logger.LogInformation($"Message {id} deleted by user {userId}");
                return Ok(new { success = true, message = "Message deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation in DeleteMessage: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting message: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error deleting message" });
            }
        }
    }
}
