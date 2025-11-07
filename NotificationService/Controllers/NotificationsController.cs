using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Send a notification to a user immediately
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received request to send notification to user: {userId}", request.UserId);

            var success = await _notificationService.SendNotificationToUserAsync(
                request.UserId,
                request.Subject,
                request.Message
            );

            if (!success)
            {
                return NotFound(new 
                { 
                    message = $"User '{request.UserId}' not found or notification failed to send" 
                });
            }

            return Ok(new 
            { 
                message = "Notification sent successfully",
                userId = request.UserId,
                subject = request.Subject
            });
        }

        /// <summary>
        /// Get all notifications for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications(string userId)
        {
            _logger.LogInformation("Fetching notifications for user: {userId}", userId);
            
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            
            return Ok(new
            {
                userId,
                count = notifications.Count(),
                notifications
            });
        }

        /// <summary>
        /// Get pending notifications (for debugging/monitoring)
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetPendingNotifications()
        {
            _logger.LogInformation("Fetching pending notifications");
            
            var notifications = await _notificationService.GetPendingNotificationsAsync();
            
            return Ok(new
            {
                count = notifications.Count(),
                notifications
            });
        }

        /// <summary>
        /// Create a scheduled notification (won't send immediately, will be picked up by polling service)
        /// </summary>
        [HttpPost("schedule")]
        public async Task<ActionResult<Notification>> ScheduleNotification([FromBody] Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating scheduled notification for user: {userId}", notification.UserId);

            var created = await _notificationService.CreateNotificationAsync(notification);
            
            return CreatedAtAction(
                nameof(GetUserNotifications), 
                new { userId = created.UserId }, 
                created
            );
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "healthy",
                service = "NotificationService",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get notification statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var allNotifications = await _notificationService.GetPendingNotificationsAsync(1000);
            
            return Ok(new
            {
                totalPending = allNotifications.Count(),
                readyToSend = allNotifications.Count(n => n.ScheduledFor <= DateTime.UtcNow),
                scheduled = allNotifications.Count(n => n.ScheduledFor > DateTime.UtcNow),
                byType = allNotifications.GroupBy(n => n.Type)
                    .Select(g => new { type = g.Key.ToString(), count = g.Count() })
            });
        }
    }
}
