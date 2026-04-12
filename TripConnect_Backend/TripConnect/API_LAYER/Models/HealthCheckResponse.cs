namespace API_LAYER.Models
{
    /// <summary>
    /// Health Check Response Model
    /// </summary>
    public class HealthCheckResponse
    {
        public string Status { get; set; } = "Unknown";
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; }
        public Dictionary<string, object> Details { get; set; }

        public HealthCheckResponse()
        {
            Timestamp = DateTime.UtcNow;
            Version = "1.0.0";
            Details = new Dictionary<string, object>();
        }
    }
}
