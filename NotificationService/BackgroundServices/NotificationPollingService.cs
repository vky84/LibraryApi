using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Services;

namespace NotificationService.BackgroundServices
{
    public class NotificationPollingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationPollingService> _logger;
        private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(5); // Poll every 5 minutes

        public NotificationPollingService(
            IServiceProvider serviceProvider,
            ILogger<NotificationPollingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("=== Notification Polling Service Started ===");
            _logger.LogInformation("Polling interval: {interval} minutes", _pollInterval.TotalMinutes);

            // Wait a bit before starting (let the app initialize)
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting notification polling cycle...");
                    await ProcessPendingNotificationsAsync(stoppingToken);
                    await CheckForOverdueRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification polling cycle");
                }

                _logger.LogInformation("Next polling cycle in {minutes} minutes", _pollInterval.TotalMinutes);
                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("=== Notification Polling Service Stopped ===");
        }

        private async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Find notifications ready to send
            var pendingNotifications = await context.Notifications
                .Where(n => !n.IsSent 
                         && n.ScheduledFor <= DateTime.UtcNow
                         && n.RetryCount < 3)
                .Take(100)
                .ToListAsync(cancellationToken);

            if (pendingNotifications.Count == 0)
            {
                _logger.LogInformation("No pending notifications found");
                return;
            }

            _logger.LogInformation("Found {count} pending notifications to process", pendingNotifications.Count);

            foreach (var notification in pendingNotifications)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await emailService.SendEmailAsync(
                        notification.UserEmail,
                        notification.Subject,
                        notification.Message
                    );

                    notification.IsSent = true;
                    notification.SentAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("? Sent notification {id} to {email}: {subject}", 
                        notification.Id, notification.UserEmail, notification.Subject);
                }
                catch (Exception ex)
                {
                    notification.RetryCount++;
                    notification.ErrorMessage = ex.Message;
                    
                    _logger.LogError(ex, "? Failed to send notification {id} (retry {retry}/3)", 
                        notification.Id, notification.RetryCount);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {count} notifications", pendingNotifications.Count);
        }

        private async Task CheckForOverdueRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

            _logger.LogInformation("Checking for overdue book reminders...");

            // Find overdue books that haven't been reminded in the last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            
            var overdueRecords = await context.BorrowingRecords
                .Where(br => !br.ReturnedDate.HasValue && br.DueDate < DateTime.UtcNow)
                .Join(context.Users,
                      br => br.UserId,
                      u => u.UserId,
                      (br, u) => new { BorrowingRecord = br, User = u })
                .Join(context.Books,
                      combined => combined.BorrowingRecord.BookId,
                      b => b.Id,
                      (combined, b) => new { combined.BorrowingRecord, combined.User, Book = b })
                .ToListAsync(cancellationToken);

            if (overdueRecords.Count == 0)
            {
                _logger.LogInformation("No overdue books found");
                return;
            }

            _logger.LogInformation("Found {count} overdue borrowing records", overdueRecords.Count);

            foreach (var record in overdueRecords)
            {
                // Check if we already sent an overdue notice today
                var existingNotification = await context.Notifications
                    .Where(n => n.BorrowingRecordId == record.BorrowingRecord.Id
                             && n.Type == Models.NotificationType.OverdueNotice
                             && n.CreatedAt >= yesterday)
                    .AnyAsync(cancellationToken);

                if (existingNotification)
                {
                    _logger.LogDebug("Overdue notice already sent for borrowing {id}", record.BorrowingRecord.Id);
                    continue;
                }

                // Create overdue notification
                var daysOverdue = (DateTime.UtcNow - record.BorrowingRecord.DueDate).Days;
                var notification = new Models.Notification
                {
                    UserId = record.User.UserId,
                    UserEmail = record.User.Email,
                    UserName = record.User.UserName,
                    Type = Models.NotificationType.OverdueNotice,
                    Subject = "?? Book Overdue - Please Return",
                    Message = $@"
                        <h2>Book Overdue Reminder</h2>
                        <p>Dear {record.User.FullName},</p>
                        <p>The following book is <strong>{daysOverdue} day(s) overdue</strong>:</p>
                        <ul>
                            <li><strong>Title:</strong> {record.Book.Title}</li>
                            <li><strong>Author:</strong> {record.Book.Author}</li>
                            <li><strong>Due Date:</strong> {record.BorrowingRecord.DueDate:yyyy-MM-dd}</li>
                        </ul>
                        <p>Please return this book as soon as possible to avoid additional late fees.</p>
                        <p>Thank you,<br/>Library Management System</p>
                    ",
                    BookId = record.Book.Id,
                    BorrowingRecordId = record.BorrowingRecord.Id,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledFor = DateTime.UtcNow
                };

                context.Notifications.Add(notification);
                _logger.LogInformation("Created overdue notice for user {userId}, book: {title}", 
                    record.User.UserId, record.Book.Title);
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Overdue reminder check complete");
        }
    }
}
