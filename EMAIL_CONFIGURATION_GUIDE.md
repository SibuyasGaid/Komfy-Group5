# Email Service Configuration Guide

## Overview

The Komfy Library Management System now includes a production-ready email service for sending password reset emails to users.

## Email Service Features

- ✅ Professional HTML email templates
- ✅ Password reset link generation
- ✅ Secure SMTP integration
- ✅ Token expiry warnings
- ✅ Error handling and logging
- ✅ Production-ready implementation

---

## Configuration Setup

### 1. Gmail SMTP Configuration (Recommended for Development)

#### Step 1: Enable 2-Factor Authentication
1. Go to your Google Account settings
2. Navigate to **Security**
3. Enable **2-Step Verification**

#### Step 2: Generate App Password
1. Go to [Google App Passwords](https://myaccount.google.com/apppasswords)
2. Select **Mail** as the app
3. Select **Other (Custom name)** as the device
4. Name it "Komfy Library"
5. Click **Generate**
6. Copy the 16-character app password

#### Step 3: Update appsettings.json
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-16-char-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Komfy Library",
  "EnableSsl": "true"
}
```

---

### 2. Other Email Providers

#### Microsoft Outlook/Office 365
```json
"EmailSettings": {
  "SmtpServer": "smtp.office365.com",
  "SmtpPort": "587",
  "SmtpUsername": "your-email@outlook.com",
  "SmtpPassword": "your-password",
  "FromEmail": "your-email@outlook.com",
  "FromName": "Komfy Library",
  "EnableSsl": "true"
}
```

#### SendGrid (Recommended for Production)
```json
"EmailSettings": {
  "SmtpServer": "smtp.sendgrid.net",
  "SmtpPort": "587",
  "SmtpUsername": "apikey",
  "SmtpPassword": "your-sendgrid-api-key",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "Komfy Library",
  "EnableSsl": "true"
}
```

#### Custom SMTP Server
```json
"EmailSettings": {
  "SmtpServer": "mail.yourdomain.com",
  "SmtpPort": "587",
  "SmtpUsername": "your-username",
  "SmtpPassword": "your-password",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "Komfy Library",
  "EnableSsl": "true"
}
```

---

## Testing the Email Service

### 1. Local Testing with Gmail

1. **Configure appsettings.json** with your Gmail credentials
2. **Run the application**
3. **Navigate to** `/Account/ForgotPassword`
4. **Enter a test email** (use your own email to receive the reset link)
5. **Check your inbox** for the password reset email

### 2. Expected Email Content

You should receive an email with:
- Professional HTML template
- "Reset Password" button with the reset link
- Plain text link as backup
- Security warning about link expiry (1 hour)
- Komfy Library branding

### 3. Testing the Reset Flow

1. **Click the "Reset Password" button** in the email
2. **You'll be redirected to** `/Account/ResetPassword?token={token}`
3. **Enter new password** and confirm
4. **Submit** to reset your password
5. **Login** with the new password

---

## Production Deployment

### Security Best Practices

1. **Use Environment Variables**
   ```bash
   # In production, set these as environment variables
   export EmailSettings__SmtpUsername="your-email@domain.com"
   export EmailSettings__SmtpPassword="your-secure-password"
   ```

2. **Use Azure Key Vault** (for Azure deployments)
   - Store SMTP credentials in Azure Key Vault
   - Reference them in appsettings.json
   - Never commit credentials to source control

3. **Use SendGrid or AWS SES** (Recommended for Production)
   - Better deliverability
   - Email analytics
   - Spam prevention
   - Higher sending limits

### Environment-Specific Configuration

**appsettings.Development.json**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "dev-email@gmail.com",
    "SmtpPassword": "dev-app-password",
    "FromEmail": "dev@komfy.com",
    "FromName": "Komfy Library (Dev)",
    "EnableSsl": "true"
  }
}
```

**appsettings.Production.json**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": "587",
    "SmtpUsername": "apikey",
    "SmtpPassword": "${SENDGRID_API_KEY}",
    "FromEmail": "noreply@komfy.com",
    "FromName": "Komfy Library",
    "EnableSsl": "true"
  }
}
```

---

## Troubleshooting

### Common Issues

#### 1. "Authentication failed" Error
**Solution**:
- Ensure 2FA is enabled for Gmail
- Use App Password, not your regular password
- Check that SmtpUsername and SmtpPassword are correct

#### 2. "SMTP server requires a secure connection"
**Solution**:
- Set `"EnableSsl": "true"` in configuration
- Use port 587 (TLS) or 465 (SSL)

#### 3. "Timed out" Error
**Solution**:
- Check your firewall settings
- Ensure outbound port 587 is open
- Try port 465 if 587 doesn't work

#### 4. Emails going to Spam
**Solution**:
- Use a professional email service (SendGrid, AWS SES)
- Configure SPF, DKIM, and DMARC records for your domain
- Use a verified sender email address

---

## Email Template Customization

The email template is located in `EmailService.cs`. To customize:

1. **Modify the HTML** in `SendPasswordResetEmailAsync` method
2. **Update colors** to match your branding
3. **Change logo** by updating the image source
4. **Adjust messaging** in the email body

---

## API Reference

### IEmailService Interface

```csharp
Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl);
Task SendEmailAsync(string toEmail, string subject, string body);
```

### Usage Example

```csharp
// Inject IEmailService in your controller
private readonly IEmailService _emailService;

// Send password reset email
var resetUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);
await _emailService.SendPasswordResetEmailAsync(userEmail, token, resetUrl);

// Send custom email
await _emailService.SendEmailAsync(
    "user@example.com",
    "Welcome to Komfy",
    "<h1>Welcome!</h1><p>Thank you for registering.</p>"
);
```

---

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review application logs in the output window
3. Verify SMTP credentials are correct
4. Test with a different email provider

---

## Security Notes

- ⚠️ Never commit SMTP credentials to source control
- ⚠️ Use App Passwords for Gmail (not your regular password)
- ⚠️ Enable SSL/TLS for secure email transmission
- ⚠️ Rotate credentials regularly
- ⚠️ Monitor for suspicious activity
- ⚠️ Use environment variables in production

---

**Version**: 1.0
**Last Updated**: 2025-01-08
