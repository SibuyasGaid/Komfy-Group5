using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Background service that periodically checks for overdue and almost-overdue books
    /// and sends email notifications to users.
    /// </summary>
    public class OverdueNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueNotificationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check once per day

        public OverdueNotificationService(
            IServiceProvider serviceProvider,
            ILogger<OverdueNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Overdue Notification Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendNotifications();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for overdue books.");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Overdue Notification Service stopped.");
        }

        private async Task CheckAndSendNotifications()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var borrowingRepository = scope.ServiceProvider.GetRequiredService<IBorrowingRepository>();
                var borrowingService = scope.ServiceProvider.GetRequiredService<IBorrowingService>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var activeBorrowings = borrowingRepository.GetActiveBorrowings().ToList();
                var now = DateTime.Now;

                foreach (var borrowing in activeBorrowings)
                {
                    try
                    {
                        // Get user information
                        var user = userRepository.GetUserById(borrowing.UserId);
                        if (user == null || string.IsNullOrEmpty(user.Email))
                        {
                            _logger.LogWarning($"User {borrowing.UserId} not found or has no email.");
                            continue;
                        }

                        var daysUntilDue = (borrowing.DueDate - now).Days;
                        var daysOverdue = (now - borrowing.DueDate).Days;

                        // Send overdue notification if book is overdue
                        if (borrowing.DueDate < now && borrowing.Status == "Active")
                        {
                            _logger.LogInformation($"Marking borrowing {borrowing.BorrowingID} as overdue and sending notification to {user.Email} for book '{borrowing.Book?.Title}'");

                            // Mark the borrowing as overdue in the database
                            borrowingService.MarkAsOverdue(borrowing.BorrowingID);

                            // Send email notification
                            await emailService.SendOverdueWarningAsync(
                                user.Email,
                                user.Name,
                                borrowing.Book?.Title ?? "Unknown Book",
                                borrowing.DueDate,
                                daysOverdue
                            );
                        }
                        // Send almost-overdue notification if book is due in 2 days or less
                        else if (daysUntilDue <= 2 && daysUntilDue > 0)
                        {
                            _logger.LogInformation($"Sending almost-overdue notification to {user.Email} for book '{borrowing.Book?.Title}'");
                            await emailService.SendAlmostOverdueWarningAsync(
                                user.Email,
                                user.Name,
                                borrowing.Book?.Title ?? "Unknown Book",
                                borrowing.DueDate
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending notification for borrowing {borrowing.BorrowingID}");
                    }
                }

                _logger.LogInformation($"Checked {activeBorrowings.Count} active borrowings for overdue notifications.");
            }
        }
    }
}
