# Hướng dẫn cấu hình Email cho tính năng Quên Mật Khẩu

## Tổng quan
Tính năng quên mật khẩu đã được triển khai và cần cấu hình email để gửi mật khẩu mới cho người dùng.

## Cấu hình Gmail SMTP

### Bước 1: Tạo App Password cho Gmail
1. Đăng nhập vào Gmail
2. Vào **Google Account Settings** > **Security**
3. Bật **2-Step Verification** (nếu chưa bật)
4. Trong phần **Signing in to Google**, chọn **App passwords**
5. Chọn **Mail** và **Other (Custom name)**, đặt tên như "Lamp Store"
6. Copy **App Password** được tạo (16 ký tự)

### Bước 2: Cấu hình trong appsettings.json

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-16-character-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Lamp Store"
  }
}
```

### Bước 3: Cấu hình cho Development
Cập nhật `appsettings.Development.json` với cùng cấu hình email.

## Cấu hình SMTP khác

### Outlook/Hotmail
```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@outlook.com",
    "SmtpPassword": "your-password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "Lamp Store"
  }
}
```

### Custom SMTP Server
```json
{
  "Email": {
    "SmtpHost": "your-smtp-server.com",
    "SmtpPort": "587",
    "SmtpUser": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@yourcompany.com",
    "FromName": "Your Company Name"
  }
}
```

## Kiểm tra hoạt động

### 1. Test API endpoint
```bash
POST /api/Account/ForgotPassword
{
  "emailOrUsername": "test@example.com"
}
```

### 2. Các response codes
- **200 OK**: Email đã được gửi thành công
- **400 Bad Request**: Không tìm thấy user hoặc lỗi validation
- **500 Internal Server Error**: Lỗi server hoặc gửi email

### 3. Log kiểm tra
Kiểm tra logs trong thư mục `Logs/` để xem chi tiết lỗi nếu có.

## Bảo mật

### Khuyến nghị
1. **Không commit** thông tin email thật vào Git
2. Sử dụng **Environment Variables** trong production
3. Sử dụng **App Password** thay vì mật khẩu chính
4. Định kỳ thay đổi **App Password**

### Environment Variables (Production)
```bash
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUser=your-email@gmail.com
Email__SmtpPassword=your-app-password
Email__FromEmail=your-email@gmail.com
Email__FromName=Lamp Store
```

## Troubleshooting

### Lỗi thường gặp
1. **Authentication failed**: Kiểm tra App Password
2. **Connection timeout**: Kiểm tra firewall/network
3. **Invalid recipient**: Kiểm tra email trong database

### Debug
1. Kiểm tra logs trong `Logs/errors.json`
2. Test kết nối SMTP bằng telnet
3. Kiểm tra cấu hình trong appsettings.json

## Thông tin thêm
- Email template được define trong `EmailService.cs`
- Mật khẩu mới có 8 ký tự (chữ hoa, chữ thường, số, ký tự đặc biệt)
- Hệ thống hỗ trợ tìm user bằng email hoặc username 