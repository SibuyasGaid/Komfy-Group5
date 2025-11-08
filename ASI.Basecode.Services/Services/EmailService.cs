using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Load SMTP configuration from appsettings.json
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"] ?? "Komfy Library";
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl)
        {
            var subject = "Reset Your Password - Komfy Library";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
        .warning {{ background-color: #FEF3C7; border-left: 4px solid #F59E0B; padding: 10px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>We received a request to reset your password for your Komfy Library account.</p>
            <p>Click the button below to reset your password:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; background-color: #fff; padding: 10px; border: 1px solid #ddd;'>
                {resetUrl}
            </p>
            <div class='warning'>
                <strong>Security Notice:</strong> This link will expire in 1 hour. If you didn't request a password reset, please ignore this email or contact support if you have concerns.
            </div>
            <p>Best regards,<br>The Komfy Library Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Komfy Library. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromEmail, _fromName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        client.EnableSsl = _enableSsl;

                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging)
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}
