namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// SMS Service Interface — sends transactional SMS messages via Twilio
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Send a verification OTP to the phone number via Twilio Verify
        /// </summary>
        Task SendVerificationAsync(string toPhoneNumber);

        /// <summary>
        /// Check if the code entered by the user is valid via Twilio Verify
        /// </summary>
        Task<bool> CheckVerificationAsync(string toPhoneNumber, string code);
    }
}
