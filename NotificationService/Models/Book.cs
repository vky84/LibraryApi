namespace NotificationService.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string Genre { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }
}
