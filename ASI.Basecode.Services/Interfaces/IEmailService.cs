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

        /// <summary>
        /// Sends an email warning that a book is almost overdue
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name</param>
        /// <param name="bookTitle">Title of the book</param>
        /// <param name="dueDate">Due date of the book</param>
        /// <returns>Task representing the async operation</returns>
        Task SendAlmostOverdueWarningAsync(string toEmail, string userName, string bookTitle, System.DateTime dueDate);

        /// <summary>
        /// Sends an email warning that a book is overdue
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name</param>
        /// <param name="bookTitle">Title of the book</param>
        /// <param name="dueDate">Due date of the book</param>
        /// <param name="daysOverdue">Number of days the book is overdue</param>
        /// <returns>Task representing the async operation</returns>
        Task SendOverdueWarningAsync(string toEmail, string userName, string bookTitle, System.DateTime dueDate, int daysOverdue);
    }
}
