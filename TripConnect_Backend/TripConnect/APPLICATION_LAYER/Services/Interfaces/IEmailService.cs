namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Email Service Interface — sends transactional emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send an email verification link to the specified address
        /// </summary>
        Task SendEmailVerificationAsync(string toEmail, string toName, string verificationUrl);
    }
}
