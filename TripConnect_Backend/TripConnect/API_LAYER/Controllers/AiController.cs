using APPLICATION_LAYER.DTOs.Ai;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// AI-assisted trip creation endpoints. All three use the local Ollama
    /// model (text) and pollinations.ai (images), accessed server-side only.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiContentService _aiContentService;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiContentService aiContentService, ILogger<AiController> logger)
        {
            _aiContentService = aiContentService;
            _logger = logger;
        }

        /// <summary>
        /// Generate an engaging trip title and description from trip inputs.
        /// </summary>
        [HttpPost("generate-trip-content")]
        public async Task<IActionResult> GenerateTripContent([FromBody] GenerateContentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _aiContentService.GenerateTripContentAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("AI content generation error: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI content generation failed");
                return StatusCode(500, new { success = false, message = "An error occurred during AI content generation" });
            }
        }

        /// <summary>
        /// Generate a full day-by-day itinerary for a trip.
        /// </summary>
        [HttpPost("generate-itinerary")]
        public async Task<IActionResult> GenerateItinerary([FromBody] GenerateItineraryDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _aiContentService.GenerateItineraryAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("AI itinerary generation error: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI itinerary generation failed");
                return StatusCode(500, new { success = false, message = "An error occurred during AI itinerary generation" });
            }
        }

        /// <summary>
        /// Generate a hero banner image for a trip.
        /// </summary>
        [HttpPost("generate-banner")]
        public async Task<IActionResult> GenerateBanner([FromBody] GenerateBannerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _aiContentService.GenerateBannerAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("AI banner generation error: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI banner generation failed");
                return StatusCode(500, new { success = false, message = "An error occurred during AI banner generation" });
            }
        }
    }
}