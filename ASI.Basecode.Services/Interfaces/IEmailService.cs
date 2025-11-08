using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends a password reset email to the specified email address
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="resetToken">Password reset token</param>
        /// <param name="resetUrl">Full URL for password reset</param>
        /// <returns>Task representing the async operation</returns>
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl);

        /// <summary>
        /// Sends a generic email
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body (HTML supported)</param>
        /// <returns>Task representing the async operation</returns>
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
