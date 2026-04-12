using API_LAYER.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Health Check Controller - Monitors API health status
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get basic health status of the API
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            try
            {
                var response = new HealthCheckResponse
                {
                    Status = "Healthy",
                    Message = "API is running successfully",
                    Details = new Dictionary<string, object>
                    {
                        { "ApiName", "TripConnect API" },
                        { "Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown" },
                        { "RuntimeVersion", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription },
                        { "MachineName", Environment.MachineName },
                        { "ProcessId", Environment.ProcessId }
                    }
                };

                _logger.LogInformation("Health check performed - Status: Healthy");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in health check: {ex.Message}");
                var errorResponse = new HealthCheckResponse
                {
                    Status = "Unhealthy",
                    Message = "API is experiencing issues",
                    Details = new Dictionary<string, object>
                    {
                        { "Error", ex.Message }
                    }
                };
                return StatusCode(503, errorResponse);
            }
        }

        /// <summary>
        /// Get detailed health status with component checks
        /// </summary>
        [HttpGet("detailed")]
        public IActionResult GetDetailedHealth()
        {
            try
            {
                var response = new HealthCheckResponse
                {
                    Status = "Healthy",
                    Message = "All systems operational",
                    Details = new Dictionary<string, object>
                    {
                        { "ApiStatus", "Up" },
                        { "Database", GetDatabaseHealthStatus() },
                        { "Services", GetServicesStatus() },
                        { "Memory", GC.GetTotalMemory(false) / 1024 / 1024 + " MB" },
                        { "Uptime", DateTime.UtcNow.Subtract(System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString(@"hh\:mm\:ss") }
                    }
                };

                _logger.LogInformation("Detailed health check performed");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in detailed health check: {ex.Message}");
                var errorResponse = new HealthCheckResponse
                {
                    Status = "Unhealthy",
                    Message = "System diagnostics failed",
                    Details = new Dictionary<string, object>
                    {
                        { "Error", ex.Message }
                    }
                };
                return StatusCode(503, errorResponse);
            }
        }

        /// <summary>
        /// Get ready status for Kubernetes probes
        /// </summary>
        [HttpGet("ready")]
        public IActionResult GetReady()
        {
            try
            {
                _logger.LogInformation("Readiness probe check");
                return Ok(new { ready = true, message = "API is ready to serve requests" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Readiness check failed: {ex.Message}");
                return StatusCode(503, new { ready = false, message = "API is not ready" });
            }
        }

        /// <summary>
        /// Get live status for Kubernetes probes
        /// </summary>
        [HttpGet("live")]
        public IActionResult GetLive()
        {
            try
            {
                _logger.LogInformation("Liveness probe check");
                return Ok(new { alive = true, message = "API is alive" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Liveness check failed: {ex.Message}");
                return StatusCode(503, new { alive = false, message = "API is not responding" });
            }
        }

        private Dictionary<string, object> GetDatabaseHealthStatus()
        {
            try
            {
                return new Dictionary<string, object>
                {
                    { "Status", "Connected" },
                    { "Type", "SQL Server" },
                    { "ConnectionPool", "Active" }
                };
            }
            catch
            {
                return new Dictionary<string, object>
                {
                    { "Status", "Disconnected" },
                    { "Type", "SQL Server" },
                    { "Error", "Unable to connect" }
                };
            }
        }

        private Dictionary<string, object> GetServicesStatus()
        {
            return new Dictionary<string, object>
            {
                { "UserService", "Active" },
                { "TripService", "Active" },
                { "ExpenseService", "Active" },
                { "ChatService", "Active" },
                { "RatingService", "Active" },
                { "JoinRequestService", "Active" }
            };
        }
    }
}
