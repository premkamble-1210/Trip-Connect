namespace APPLICATION_LAYER.Models
{
    /// <summary>
    /// Twilio SMS settings — bound from appsettings.json "TwilioSettings" section
    /// </summary>
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string VerifyServiceSid { get; set; } = string.Empty;
    }
}
