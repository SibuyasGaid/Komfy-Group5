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

        public async Task SendAlmostOverdueWarningAsync(string toEmail, string userName, string bookTitle, DateTime dueDate)
        {
            var subject = "Reminder: Book Due Soon - Komfy Library";
            var daysUntilDue = (dueDate - DateTime.Now).Days;

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #F59E0B; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .book-info {{ background-color: #fff; padding: 15px; border-left: 4px solid #F59E0B; margin: 20px 0; }}
        .warning {{ background-color: #FEF3C7; border-left: 4px solid #F59E0B; padding: 10px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
        .icon {{ font-size: 48px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 Book Due Soon</h1>
        </div>
        <div class='content'>
            <p>Hello {userName},</p>
            <p>This is a friendly reminder that you have a borrowed book that is due soon.</p>
            <div class='book-info'>
                <h3 style='margin-top: 0; color: #F59E0B;'>Book Details:</h3>
                <p><strong>Title:</strong> {bookTitle}</p>
                <p><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</p>
                <p><strong>Days Remaining:</strong> {daysUntilDue} day{(daysUntilDue != 1 ? "s" : "")}</p>
            </div>
            <div class='warning'>
                <strong>⚠️ Action Required:</strong> Please return or renew this book before the due date to avoid late fees.
            </div>
            <p>You can return the book to any library location during operating hours.</p>
            <p>Thank you for using Komfy Library!</p>
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

        public async Task SendOverdueWarningAsync(string toEmail, string userName, string bookTitle, DateTime dueDate, int daysOverdue)
        {
            var subject = "URGENT: Overdue Book - Komfy Library";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #EF4444; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .book-info {{ background-color: #fff; padding: 15px; border-left: 4px solid #EF4444; margin: 20px 0; }}
        .urgent {{ background-color: #FEE2E2; border-left: 4px solid #EF4444; padding: 15px; margin: 20px 0; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ OVERDUE BOOK NOTICE</h1>
        </div>
        <div class='content'>
            <p>Hello {userName},</p>
            <p>This is an urgent notice that you have an overdue book.</p>
            <div class='book-info'>
                <h3 style='margin-top: 0; color: #EF4444;'>Book Details:</h3>
                <p><strong>Title:</strong> {bookTitle}</p>
                <p><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</p>
                <p><strong>Days Overdue:</strong> {daysOverdue} day{(daysOverdue != 1 ? "s" : "")}</p>
            </div>
            <div class='urgent'>
                <p style='margin: 0;'>🚨 IMMEDIATE ACTION REQUIRED</p>
                <p style='margin: 10px 0 0 0;'>Please return this book as soon as possible. Late fees may apply.</p>
            </div>
            <p><strong>What to do:</strong></p>
            <ul>
                <li>Return the book to any library location immediately</li>
                <li>Contact us if you need assistance or have questions</li>
                <li>Check your account for any applicable late fees</li>
            </ul>
            <p>We appreciate your prompt attention to this matter.</p>
            <p>Best regards,<br>The Komfy Library Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Komfy Library. All rights reserved.</p>
            <p>If you have already returned this book, please disregard this notice.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
