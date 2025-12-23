using Lssctc.ProgramManagement.Accounts.Authens.Dtos;
using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Accounts.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Accounts.Authens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IUsersService _usersService;
        private readonly IOtpService _otpService;

        public PasswordController(
            IMailService mailService,
            IUsersService usersService,
            IOtpService otpService)
        {
            _mailService = mailService;
            _usersService = usersService;
            _otpService = otpService;
        }

        /// <summary>
        /// Request OTP for password reset with rate limiting (60 seconds cooldown)
        /// </summary>
        /// <param name="request">Email address for password reset</param>
        /// <returns>Success message with OTP sent or rate limit error</returns>
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                // Step 1: Check if email exists
                bool emailExists = await _usersService.IsEmailExistsAsync(request.Email);
                if (!emailExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email does not exist in the system"
                    });
                }

                // Step 2: Generate OTP with rate limiting
                var otpResult = await _otpService.GenerateOtpAsync(request.Email);

                // Check if rate limited
                if (!otpResult.Success)
                {
                    return StatusCode(429, new
                    {
                        success = false,
                        message = otpResult.Message
                    });
                }

                // Step 3: Send OTP via email
                string subject = "Mã OTP - Đặt lại mật khẩu";
                string htmlBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); overflow: hidden; }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px; }}
                            .otp-box {{ background-color: #f8f9fa; border: 2px dashed #667eea; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
                            .otp-code {{ font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; margin: 10px 0; }}
                            .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
                            .warning-icon {{ color: #ffc107; font-weight: bold; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🔑 Đặt lại mật khẩu</h1>
                                <p style='margin: 10px 0 0 0; font-size: 14px;'>Hệ thống Quản lý Đào tạo LSSCTC</p>
                            </div>
                            <div class='content'>
                                <h2 style='color: #333; margin-top: 0;'>Xin chào!</h2>
                                <p style='color: #555; line-height: 1.6;'>
                                    Bạn đã yêu cầu đặt lại mật khẩu tài khoản. 
                                    Vui lòng sử dụng mã OTP bên dưới để xác minh danh tính của bạn:
                                </p>
                                
                                <div class='otp-box'>
                                    <p style='margin: 0; color: #666; font-size: 14px;'>Mã OTP của bạn</p>
                                    <div class='otp-code'>{otpResult.OtpCode}</div>
                                    <p style='margin: 10px 0 0 0; color: #666; font-size: 12px;'>Nhập mã này vào biểu mẫu xác minh</p>
                                </div>

                                <div class='warning'>
                                    <p style='margin: 0;'><span class='warning-icon'>⚠️</span> <strong>Lưu ý quan trọng:</strong></p>
                                    <ul style='margin: 10px 0 0 20px; color: #856404;'>
                                        <li>Mã OTP này chỉ có hiệu lực trong <strong>5 phút</strong></li>
                                        <li>Không chia sẻ mã này cho bất kỳ ai</li>
                                        <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                                    </ul>
                                </div>

                                <p style='color: #555; margin-top: 30px;'>
                                    Nếu bạn gặp bất kỳ vấn đề nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.
                                </p>
                            </div>
                            <div class='footer'>
                                <p style='margin: 0;'>
                                    © 2024 Trung tâm Đào tạo LSSCTC<br>
                                    Đây là email tự động. Vui lòng không trả lời.
                                </p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _mailService.SendEmailAsync(request.Email, subject, htmlBody);

                return Ok(new
                {
                    success = true,
                    message = "OTP code has been sent to your email. Please check your inbox.",
                    expiresIn = 300 // 5 minutes in seconds
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to send OTP code",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify OTP code and receive reset token
        /// </summary>
        /// <param name="request">Email and OTP code</param>
        /// <returns>Success message with reset token</returns>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            try
            {
                var result = await _otpService.VerifyOtpAsync(request.Email, request.OtpCode);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    email = result.Email,
                    resetToken = result.ResetToken,
                    expiresIn = 600 // Reset token valid for 10 minutes
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP verification failed",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        /// <param name="request">Email, reset token, new password, and confirm password</param>
        /// <returns>Success or error message</returns>
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                // Step 1: Validate reset token
                var tokenValidation = await _otpService.ValidateResetTokenAsync(request.Email, request.ResetToken);

                if (!tokenValidation.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = tokenValidation.Message
                    });
                }

                // Step 2: Reset password in database
                await _usersService.ResetPasswordByEmailAsync(request.Email, request.NewPassword);

                return Ok(new
                {
                    success = true,
                    message = "Password has been reset successfully. You can now login with your new password."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to reset password",
                    error = ex.Message
                });
            }
        }
    }
}
