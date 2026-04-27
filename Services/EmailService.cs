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
                            <h2>Đặt lại mật khẩu - CapyLumine</h2>
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
                            <p>Trân trọng,<br>Đội ngũ CapyLumine</p>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(Models.OrderModel order, string storeUrl = "https://capylumine.com")
        {
            if (string.IsNullOrEmpty(order.Email)) return false;

            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "khongthaydoi124@gmail.com";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "rwaa cexk nbif pvia";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
                var fromName = _configuration["Email:FromName"] ?? "CapyLumine";

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = $"Xác nhận đơn hàng #{order.Id.ToString()?[..8].ToUpper()} - CapyLumine",
                    Body = GenerateOrderConfirmationEmailBody(order, storeUrl),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(order.Email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Order confirmation email sent successfully to {order.Email} for order {order.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order confirmation email to {order.Email}");
                return false;
            }
        }

        private string FormatSelectedOptions(string? optionsJson)
        {
            if (string.IsNullOrEmpty(optionsJson)) return string.Empty;
            try
            {
                var options = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(optionsJson);
                if (options == null) return optionsJson;

                var formattedList = new List<string>();
                foreach (var kvp in options)
                {
                    if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.Object && kvp.Value.TryGetProperty("value", out var valueProp))
                    {
                        formattedList.Add($"{kvp.Key}: {valueProp.GetString()}");
                    }
                    else if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        formattedList.Add($"{kvp.Key}: {kvp.Value.GetString()}");
                    }
                    else
                    {
                        formattedList.Add($"{kvp.Key}: {kvp.Value.ToString()}");
                    }
                }
                return string.Join(" | ", formattedList);
            }
            catch
            {
                return optionsJson;
            }
        }

        private string GenerateOrderConfirmationEmailBody(Models.OrderModel order, string storeUrl)
        {
            var itemsHtml = "";
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    var formattedOptions = FormatSelectedOptions(item.SelectedOptions);
                    var optionsText = string.IsNullOrEmpty(formattedOptions) ? "" : $"<br><small style='color: #6c757d;'>Phân loại: {formattedOptions}</small>";
                    itemsHtml += $@"
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>
                                <strong>{item.ProductName}</strong>{optionsText}
                            </td>
                            <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: center;'>{item.Quantity}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{item.Price:N0}đ</td>
                            <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{(item.Quantity * item.Price):N0}đ</td>
                        </tr>";
                }
            }

            var paymentMethod = order.PaymentMethod.ToLower() == "cod" ? "Thanh toán khi nhận hàng (COD)" : 
                               (order.PaymentMethod.ToLower() == "vnpay" ? "Thanh toán qua VNPay" : order.PaymentMethod);

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 650px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .brand-name {{ color: #FFB02E; font-size: 28px; font-weight: bold; margin: 0; }}
                        .content {{ padding: 20px; border: 1px solid #e9ecef; border-top: none; }}
                        .section-title {{ font-size: 18px; font-weight: bold; margin-bottom: 15px; padding-bottom: 8px; border-bottom: 2px solid #f8f9fa; }}
                        .info-table {{ width: 100%; margin-bottom: 25px; }}
                        .info-table td {{ padding: 5px 0; vertical-align: top; }}
                        .info-label {{ font-weight: bold; width: 140px; color: #555; }}
                        .items-table {{ width: 100%; border-collapse: collapse; margin-bottom: 25px; }}
                        .items-table th {{ background-color: #f8f9fa; padding: 12px 10px; text-align: left; border-bottom: 2px solid #dee2e6; }}
                        .summary-table {{ width: 100%; margin-bottom: 25px; }}
                        .summary-table td {{ padding: 8px 0; }}
                        .summary-total {{ font-size: 18px; font-weight: bold; color: #dc3545; }}
                        .btn {{ display: inline-block; padding: 12px 25px; background-color: #FFB02E; color: #fff; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #6c757d; border-radius: 0 0 8px 8px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1 class='brand-name'>CapyLumine</h1>
                            <p style='margin: 5px 0 0 0; font-size: 16px;'>Xác nhận đơn hàng thành công!</p>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{order.FullName}</strong>,</p>
                            <p>Cảm ơn bạn đã tin tưởng và mua sắm tại CapyLumine. Đơn hàng của bạn đã được hệ thống ghi nhận thành công và đang được xử lý.</p>
                            
                            <div class='section-title'>Thông tin đơn hàng</div>
                            <table class='info-table'>
                                <tr>
                                    <td class='info-label'>Mã đơn hàng:</td>
                                    <td><strong>#{order.Id.ToString()?[..8].ToUpper()}</strong></td>
                                </tr>
                                <tr>
                                    <td class='info-label'>Ngày đặt:</td>
                                    <td>{order.OrderDate:dd/MM/yyyy HH:mm}</td>
                                </tr>
                                <tr>
                                    <td class='info-label'>Trạng thái:</td>
                                    <td><span style='background-color: #fff3cd; color: #856404; padding: 3px 8px; border-radius: 4px; font-size: 12px; font-weight: bold;'>Đang chờ xử lý</span></td>
                                </tr>
                            </table>

                            <div class='section-title'>Thông tin giao hàng</div>
                            <table class='info-table'>
                                <tr>
                                    <td class='info-label'>Người nhận:</td>
                                    <td>{order.FullName}</td>
                                </tr>
                                <tr>
                                    <td class='info-label'>Số điện thoại:</td>
                                    <td>{order.Phone}</td>
                                </tr>
                                <tr>
                                    <td class='info-label'>Địa chỉ:</td>
                                    <td>{order.Address}, {order.Ward}, {order.District}, {order.City}</td>
                                </tr>
                                {(string.IsNullOrEmpty(order.Note) ? "" : $"<tr><td class='info-label'>Ghi chú:</td><td>{order.Note}</td></tr>")}
                            </table>

                            <div class='section-title'>Chi tiết sản phẩm</div>
                            <table class='items-table'>
                                <thead>
                                    <tr>
                                        <th>Sản phẩm</th>
                                        <th style='text-align: center; width: 60px;'>SL</th>
                                        <th style='text-align: right; width: 100px;'>Đơn giá</th>
                                        <th style='text-align: right; width: 100px;'>Thành tiền</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                </tbody>
                            </table>

                            <div style='border-top: 1px solid #dee2e6; margin: 20px 0;'></div>
                            
                            <table class='summary-table'>
                                <tr>
                                    <td colspan='2' style='text-align: right; width: 70%;'>Tổng tiền hàng:</td>
                                    <td style='text-align: right;'>{(order.TotalAmount - order.ShippingFee):N0}đ</td>
                                </tr>
                                <tr>
                                    <td colspan='2' style='text-align: right;'>Phí vận chuyển:</td>
                                    <td style='text-align: right;'>{order.ShippingFee:N0}đ</td>
                                </tr>
                                <tr>
                                    <td colspan='2' style='text-align: right;'>Phương thức thanh toán:</td>
                                    <td style='text-align: right;'>{paymentMethod}</td>
                                </tr>
                                <tr>
                                    <td colspan='2' style='text-align: right; font-weight: bold;'>Tổng thanh toán:</td>
                                    <td style='text-align: right;' class='summary-total'>{order.TotalAmount:N0}đ</td>
                                </tr>
                            </table>

                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='{storeUrl}/my-orders' class='btn'>Xem đơn hàng trên Website</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Đây là email tự động, vui lòng không trả lời qua email này.</p>
                            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ bộ phận CSKH qua Hotline hoặc Fanpage.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public async Task<bool> SendNewOrderNotificationToAdminAsync(Models.OrderModel order, IEnumerable<string> adminEmails, string storeUrl = "https://capylumine.com")
        {
            var emailList = adminEmails?.Where(e => !string.IsNullOrEmpty(e)).ToList();
            if (emailList == null || !emailList.Any()) return false;

            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "khongthaydoi124@gmail.com";
                var smtpPassword = _configuration["Email:SmtpPassword"] ?? "rwaa cexk nbif pvia";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
                var fromName = _configuration["Email:FromName"] ?? "CapyLumine";

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                var orderCode = order.Id.ToString()?[..8].ToUpper();
                var isGuest = string.IsNullOrEmpty(order.UserId);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = $"🛒 Đơn hàng mới #{orderCode} - {order.TotalAmount:N0}đ{(isGuest ? " (Khách vãng lai)" : "")}",
                    Body = GenerateAdminOrderNotificationEmailBody(order, storeUrl),
                    IsBodyHtml = true
                };

                foreach (var email in emailList)
                {
                    mailMessage.To.Add(email);
                }

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Admin order notification email sent to {string.Join(", ", emailList)} for order {order.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send admin order notification for order {order.Id}");
                return false;
            }
        }

        private string GenerateAdminOrderNotificationEmailBody(Models.OrderModel order, string storeUrl)
        {
            var orderCode = order.Id.ToString()?[..8].ToUpper();
            var isGuest = string.IsNullOrEmpty(order.UserId);

            var itemsHtml = "";
            var totalItems = 0;
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    totalItems += item.Quantity;
                    var formattedOptions = FormatSelectedOptions(item.SelectedOptions);
                    var optionsText = string.IsNullOrEmpty(formattedOptions) ? "" : $"<br><small style='color: #6c757d;'>Phân loại: {formattedOptions}</small>";
                    itemsHtml += $@"
                        <tr>
                            <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>
                                <strong>{item.ProductName}</strong>{optionsText}
                            </td>
                            <td style='padding: 8px; border-bottom: 1px solid #dee2e6; text-align: center;'>{item.Quantity}</td>
                            <td style='padding: 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>{item.Price:N0}đ</td>
                            <td style='padding: 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>{(item.Quantity * item.Price):N0}đ</td>
                        </tr>";
                }
            }

            var paymentMethod = order.PaymentMethod.ToLower() == "cod" ? "COD" :
                               (order.PaymentMethod.ToLower() == "bank" ? "Chuyển khoản" : order.PaymentMethod);

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 650px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #e8453c, #ff6b6b); padding: 20px; text-align: center; border-radius: 8px 8px 0 0; color: white; }}
                        .content {{ padding: 20px; border: 1px solid #e9ecef; border-top: none; }}
                        .badge {{ display: inline-block; padding: 4px 10px; border-radius: 12px; font-size: 12px; font-weight: bold; }}
                        .badge-guest {{ background: #ffc107; color: #333; }}
                        .badge-member {{ background: #28a745; color: white; }}
                        .info-grid {{ display: table; width: 100%; margin-bottom: 15px; }}
                        .info-row {{ display: table-row; }}
                        .info-label {{ display: table-cell; padding: 4px 10px 4px 0; font-weight: bold; color: #555; width: 130px; }}
                        .info-value {{ display: table-cell; padding: 4px 0; }}
                        .items-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .items-table th {{ background-color: #f8f9fa; padding: 10px 8px; text-align: left; border-bottom: 2px solid #dee2e6; font-size: 13px; }}
                        .total-row {{ font-size: 20px; font-weight: bold; color: #e8453c; }}
                        .btn {{ display: inline-block; padding: 10px 20px; background-color: #e8453c; color: #fff; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .footer {{ background-color: #f8f9fa; padding: 12px; text-align: center; font-size: 11px; color: #999; border-radius: 0 0 8px 8px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2 style='margin: 0;'>🛒 Đơn hàng mới!</h2>
                            <p style='margin: 5px 0 0; font-size: 22px; font-weight: bold;'>#{orderCode}</p>
                        </div>
                        <div class='content'>
                            <p style='margin-top: 0;'>
                                <span class='badge {(isGuest ? "badge-guest" : "badge-member")}'>{(isGuest ? "👤 Khách vãng lai" : "✅ Thành viên")}</span>
                                &nbsp; {order.OrderDate:dd/MM/yyyy HH:mm}
                            </p>

                            <h3 style='margin-bottom: 8px; border-bottom: 2px solid #f0f0f0; padding-bottom: 6px;'>📋 Thông tin khách hàng</h3>
                            <div class='info-grid'>
                                <div class='info-row'><span class='info-label'>Họ tên:</span><span class='info-value'><strong>{order.FullName}</strong></span></div>
                                <div class='info-row'><span class='info-label'>SĐT:</span><span class='info-value'>{order.Phone}</span></div>
                                {(string.IsNullOrEmpty(order.Email) ? "" : $"<div class='info-row'><span class='info-label'>Email:</span><span class='info-value'>{order.Email}</span></div>")}
                                <div class='info-row'><span class='info-label'>Địa chỉ:</span><span class='info-value'>{order.Address}, {order.Ward}, {order.District}, {order.City}</span></div>
                                {(string.IsNullOrEmpty(order.Note) ? "" : $"<div class='info-row'><span class='info-label'>Ghi chú:</span><span class='info-value' style='color: #e8453c;'><strong>{order.Note}</strong></span></div>")}
                                <div class='info-row'><span class='info-label'>Thanh toán:</span><span class='info-value'>{paymentMethod}</span></div>
                            </div>

                            <h3 style='margin-bottom: 8px; border-bottom: 2px solid #f0f0f0; padding-bottom: 6px;'>📦 Sản phẩm ({totalItems} sản phẩm)</h3>
                            <table class='items-table'>
                                <thead>
                                    <tr>
                                        <th>Sản phẩm</th>
                                        <th style='text-align: center; width: 40px;'>SL</th>
                                        <th style='text-align: right; width: 90px;'>Đơn giá</th>
                                        <th style='text-align: right; width: 90px;'>Thành tiền</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                </tbody>
                            </table>

                            <table style='width: 100%; margin-top: 10px;'>
                                <tr>
                                    <td style='text-align: right; padding: 4px 0;'>Tiền hàng:</td>
                                    <td style='text-align: right; width: 120px; padding: 4px 0;'>{(order.TotalAmount - order.ShippingFee):N0}đ</td>
                                </tr>
                                <tr>
                                    <td style='text-align: right; padding: 4px 0;'>Phí ship:</td>
                                    <td style='text-align: right; padding: 4px 0;'>{(order.ShippingFee == 0 ? "Miễn phí" : $"{order.ShippingFee:N0}đ")}</td>
                                </tr>
                                <tr>
                                    <td style='text-align: right; padding: 8px 0; font-weight: bold;'>TỔNG:</td>
                                    <td style='text-align: right; padding: 8px 0;' class='total-row'>{order.TotalAmount:N0}đ</td>
                                </tr>
                            </table>

                            <div style='text-align: center; margin-top: 20px;'>
                                <a href='{storeUrl}/admin/orders' class='btn'>Xem trên Admin Panel</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Email thông báo tự động từ hệ thống CapyLumine</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}