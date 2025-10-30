namespace LibraryApi.Models
{
    public class BorrowBookRequest
    {
        public int BookId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}