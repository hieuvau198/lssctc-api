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
        public string? ResetToken { get; set; }
    }

    /// <summary>
    /// Result object for reset token validation
    /// </summary>
    public class ResetTokenValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
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
        /// Verify OTP code for given email and generate reset token
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="otpCode">OTP code to verify</param>
        /// <returns>Verification result with reset token</returns>
        Task<OtpVerificationResult> VerifyOtpAsync(string email, string otpCode);

        /// <summary>
        /// Create a secure reset token for password reset
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Generated reset token (GUID)</returns>
        Task<string> CreateResetTokenAsync(string email);

        /// <summary>
        /// Validate reset token and remove it after validation
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="resetToken">Reset token to validate</param>
        /// <returns>Validation result</returns>
        Task<ResetTokenValidationResult> ValidateResetTokenAsync(string email, string resetToken);

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
