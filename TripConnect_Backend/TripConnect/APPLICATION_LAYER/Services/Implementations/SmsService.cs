using APPLICATION_LAYER.Models;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace APPLICATION_LAYER.Services.Implementations
{
    /// <summary>
    /// SMS Service — sends and verifies OTPs via Twilio Verify (no phone number required)
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly TwilioSettings _twilioSettings;
        private readonly Serilog.ILogger _logger;

        public SmsService(IOptions<TwilioSettings> twilioSettings, Serilog.ILogger logger)
        {
            _twilioSettings = twilioSettings.Value;
            _logger = logger;
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public async Task SendVerificationAsync(string toPhoneNumber)
        {
            try
            {
                var normalizedPhone = NormalizeToE164(toPhoneNumber);

                await VerificationResource.CreateAsync(
                    to: normalizedPhone,
                    channel: "sms",
                    pathServiceSid: _twilioSettings.VerifyServiceSid
                );

                _logger.Information("Verification OTP sent to {Phone}", toPhoneNumber);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to send verification OTP to {Phone}: {Message}", toPhoneNumber, ex.Message);
                throw;
            }
        }

        public async Task<bool> CheckVerificationAsync(string toPhoneNumber, string code)
        {
            try
            {
                var normalizedPhone = NormalizeToE164(toPhoneNumber);

                var verificationCheck = await VerificationCheckResource.CreateAsync(
                    to: normalizedPhone,
                    code: code,
                    pathServiceSid: _twilioSettings.VerifyServiceSid
                );

                var approved = verificationCheck.Status == "approved";
                _logger.Information("Verification check for {Phone}: {Status}", toPhoneNumber, verificationCheck.Status);
                return approved;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to check verification for {Phone}: {Message}", toPhoneNumber, ex.Message);
                throw;
            }
        }

        private static string NormalizeToE164(string phone)
        {
            var stripped = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d+]", "");

            if (stripped.StartsWith("+"))
                return stripped;

            // 10-digit Indian mobile number
            if (stripped.Length == 10)
                return $"+91{stripped}";

            // 12-digit with country code but no +
            if (stripped.Length == 12)
                return $"+{stripped}";

            return $"+{stripped}";
        }
    }
}
