namespace APPLICATION_LAYER.Models
{
    /// <summary>
    /// Email settings configuration model — bound from appsettings.json "EmailSettings" section
    /// </summary>
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}
