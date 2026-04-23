using System.ComponentModel.DataAnnotations;

namespace APPLICATION_LAYER.DTOs.User
{
    /// <summary>
    /// DTO for verifying phone OTP submitted by the user
    /// </summary>
    public class VerifyPhoneOtpDto
    {
        /// <summary>
        /// The 6-digit OTP code the user entered
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
    }
}
