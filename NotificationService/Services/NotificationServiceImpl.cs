using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> SendNotificationToUserAsync(string userId, string subject, string message);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int limit = 100);
        Task MarkAsSentAsync(int notificationId, bool success, string? errorMessage = null);
    }

    public class NotificationServiceImpl : INotificationService
    {
        private readonly LibraryDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationServiceImpl> _logger;

        public NotificationServiceImpl(
            LibraryDbContext context, 
            IEmailService emailService,
            ILogger<NotificationServiceImpl> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            if (!notification.ScheduledFor.HasValue)
            {
                notification.ScheduledFor = DateTime.UtcNow;
            }

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created notification {id} for user {userId}", 
                notification.Id, notification.UserId);
            
            return notification;
        }

        public async Task<bool> SendNotificationToUserAsync(string userId, string subject, string message)
        {
            try
            {
                // Find user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("User {userId} not found or inactive", userId);
                    return false;
                }

                // Create notification record
                var notification = new Notification
                {
                    UserId = userId,
                    UserEmail = user.Email,
                    UserName = user.UserName,
                    Type = NotificationType.ManualNotification,
                    Subject = subject,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledFor = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send email immediately
                try
                {
                    await _emailService.SendEmailAsync(user.Email, subject, message);
                    
                    notification.IsSent = true;
                    notification.SentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Notification sent successfully to {userId} ({email})", 
                        userId, user.Email);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    notification.RetryCount++;
                    notification.ErrorMessage = ex.Message;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogError(ex, "Failed to send notification to {userId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendNotificationToUserAsync for {userId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int limit = 100)
        {
            return await _context.Notifications
                .Where(n => !n.IsSent 
                         && n.ScheduledFor <= DateTime.UtcNow
                         && n.RetryCount < 3)
                .OrderBy(n => n.ScheduledFor)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarkAsSentAsync(int notificationId, bool success, string? errorMessage = null)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return;

            if (success)
            {
                notification.IsSent = true;
                notification.SentAt = DateTime.UtcNow;
            }
            else
            {
                notification.RetryCount++;
                notification.ErrorMessage = errorMessage;
            }

            await _context.SaveChangesAsync();
        }
    }
}
