namespace NotificationService.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        // References
        public int? BookId { get; set; }
        public int? BorrowingRecordId { get; set; }
        
        // Status tracking
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }  // Send at specific time
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum NotificationType
    {
        BookBorrowed = 1,
        BookReturned = 2,
        DueSoonReminder = 3,
        OverdueNotice = 4,
        WelcomeEmail = 5,
        ManualNotification = 6
    }
}
