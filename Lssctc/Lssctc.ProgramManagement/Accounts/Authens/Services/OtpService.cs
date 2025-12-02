using Microsoft.Extensions.Caching.Memory;

namespace Lssctc.ProgramManagement.Accounts.Authens.Services
{
    /// <summary>
    /// Service for OTP generation, verification, and rate limiting
    /// </summary>
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly Random _random = new Random();

        // Configuration constants
        private const int OTP_LENGTH = 6;
        private const int OTP_EXPIRATION_MINUTES = 5;
        private const int COOLDOWN_SECONDS = 60;

        // Cache key prefixes
        private const string OTP_KEY_PREFIX = "otp_";
        private const string COOLDOWN_KEY_PREFIX = "otp_cooldown_";

        public OtpService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Generate and store OTP with rate limiting
        /// </summary>
        public async Task<OtpGenerationResult> GenerateOtpAsync(string email)
        {
            var normalizedEmail = email.ToLower();

            // Check if email is in cooldown period
            if (IsInCooldown(normalizedEmail))
            {
                var remainingSeconds = GetRemainingCooldownSeconds(normalizedEmail);
                return new OtpGenerationResult
                {
                    Success = false,
                    Message = $"Please try again in {remainingSeconds} seconds",
                    RetryAfterSeconds = remainingSeconds
                };
            }

            // Generate 6-digit OTP
            string otp = _random.Next(100000, 999999).ToString();

            // Store OTP in cache with 5 minutes expiration
            var otpCacheKey = $"{OTP_KEY_PREFIX}{normalizedEmail}";
            var otpCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(OTP_EXPIRATION_MINUTES));

            _memoryCache.Set(otpCacheKey, otp, otpCacheOptions);

            // Set cooldown period (60 seconds)
            var cooldownCacheKey = $"{COOLDOWN_KEY_PREFIX}{normalizedEmail}";
            var cooldownExpiration = DateTime.UtcNow.AddSeconds(COOLDOWN_SECONDS);
            var cooldownCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(COOLDOWN_SECONDS));

            _memoryCache.Set(cooldownCacheKey, cooldownExpiration, cooldownCacheOptions);

            return await Task.FromResult(new OtpGenerationResult
            {
                Success = true,
                OtpCode = otp,
                Message = "OTP generated successfully"
            });
        }

        /// <summary>
        /// Verify OTP code for given email
        /// </summary>
        public async Task<OtpVerificationResult> VerifyOtpAsync(string email, string otpCode)
        {
            var normalizedEmail = email.ToLower();
            var otpCacheKey = $"{OTP_KEY_PREFIX}{normalizedEmail}";

            // Try to get OTP from cache
            if (!_memoryCache.TryGetValue(otpCacheKey, out string? cachedOtp))
            {
                return await Task.FromResult(new OtpVerificationResult
                {
                    Success = false,
                    Message = "OTP code has expired or has not been sent"
                });
            }

            // Verify OTP matches
            if (cachedOtp != otpCode)
            {
                return await Task.FromResult(new OtpVerificationResult
                {
                    Success = false,
                    Message = "OTP code is incorrect"
                });
            }

            // OTP is valid, remove it from cache
            _memoryCache.Remove(otpCacheKey);

            // Also remove cooldown since OTP was verified successfully
            var cooldownCacheKey = $"{COOLDOWN_KEY_PREFIX}{normalizedEmail}";
            _memoryCache.Remove(cooldownCacheKey);

            return await Task.FromResult(new OtpVerificationResult
            {
                Success = true,
                Message = "Verification successful! You can proceed to reset your password.",
                Email = email
            });
        }

        /// <summary>
        /// Check if email is in cooldown period
        /// </summary>
        public bool IsInCooldown(string email)
        {
            var normalizedEmail = email.ToLower();
            var cooldownCacheKey = $"{COOLDOWN_KEY_PREFIX}{normalizedEmail}";

            return _memoryCache.TryGetValue(cooldownCacheKey, out DateTime _);
        }

        /// <summary>
        /// Get remaining cooldown time in seconds
        /// </summary>
        public int GetRemainingCooldownSeconds(string email)
        {
            var normalizedEmail = email.ToLower();
            var cooldownCacheKey = $"{COOLDOWN_KEY_PREFIX}{normalizedEmail}";

            if (_memoryCache.TryGetValue(cooldownCacheKey, out DateTime cooldownExpiration))
            {
                var remaining = (cooldownExpiration - DateTime.UtcNow).TotalSeconds;
                return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
            }

            return 0;
        }
    }
}
