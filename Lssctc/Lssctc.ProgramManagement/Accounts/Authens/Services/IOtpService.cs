namespace Lssctc.ProgramManagement.Accounts.Authens.Services
{
    /// <summary>
    /// Result object for OTP generation
    /// </summary>
    public class OtpGenerationResult
    {
        public bool Success { get; set; }
        public string? OtpCode { get; set; }
        public string? Message { get; set; }
        public int? RetryAfterSeconds { get; set; }
    }

    /// <summary>
    /// Result object for OTP verification
    /// </summary>
    public class OtpVerificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    /// <summary>
    /// Service interface for OTP operations with rate limiting
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Generate and store OTP with rate limiting
        /// </summary>
        /// <param name="email">Email address for OTP generation</param>
        /// <returns>OTP generation result with success status and OTP code</returns>
        Task<OtpGenerationResult> GenerateOtpAsync(string email);

        /// <summary>
        /// Verify OTP code for given email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="otpCode">OTP code to verify</param>
        /// <returns>Verification result</returns>
        Task<OtpVerificationResult> VerifyOtpAsync(string email, string otpCode);

        /// <summary>
        /// Check if email is in cooldown period
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>True if in cooldown, false otherwise</returns>
        bool IsInCooldown(string email);

        /// <summary>
        /// Get remaining cooldown time in seconds
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Remaining seconds or 0 if not in cooldown</returns>
        int GetRemainingCooldownSeconds(string email);
    }
}
