namespace NotificationService.Models
{
    public class SendNotificationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
