namespace NotificationService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MembershipType { get; set; } = "Standard"; // Standard, Premium
        public DateTime JoinedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
