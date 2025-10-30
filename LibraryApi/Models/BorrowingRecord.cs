namespace LibraryApi.Models
{
    public class BorrowingRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime BorrowedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public bool IsReturned => ReturnedDate.HasValue;
        public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;
    }
}