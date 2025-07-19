using System.Net;
using System.Net.Mail;

namespace LampStoreProjects.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string username, string newPassword)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "khongthaydoi124@gmail.com";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "rwaa cexk nbif pvia";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
                var fromName = _configuration["Email:FromName"] ?? "Lamp Store";

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Đặt lại mật khẩu - Lamp Store",
                    Body = GeneratePasswordResetEmailBody(username, newPassword),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Password reset email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                return false;
            }
        }

        private string GeneratePasswordResetEmailBody(string username, string newPassword)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .password-box {{ background-color: #e9ecef; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #6c757d; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Đặt lại mật khẩu - Lamp Store</h2>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{username}</strong>,</p>
                            <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Mật khẩu mới của bạn là:</p>
                            <div class='password-box'>
                                <strong style='font-size: 18px; color: #dc3545;'>{newPassword}</strong>
                            </div>
                            <p><strong>Lưu ý quan trọng:</strong></p>
                            <ul>
                                <li>Vui lòng đăng nhập bằng mật khẩu mới này</li>
                                <li>Để bảo mật, hãy thay đổi mật khẩu sau khi đăng nhập thành công</li>
                                <li>Không chia sẻ mật khẩu này với bất kỳ ai</li>
                            </ul>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ với chúng tôi ngay lập tức.</p>
                            <p>Trân trọng,<br>Đội ngũ Lamp Store</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
} 