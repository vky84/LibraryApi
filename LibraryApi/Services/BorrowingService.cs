using LibraryApi.Models;

namespace LibraryApi.Services
{
    public interface IBorrowingService
    {
        Task<BorrowingRecord?> BorrowBookAsync(BorrowBookRequest request);
        Task<bool> ReturnBookAsync(int borrowingId);
        Task<IEnumerable<BorrowingRecord>> GetUserBorrowingsAsync(string userId);
        Task<IEnumerable<BorrowingRecord>> GetOverdueBooksAsync();
    }

    public class BorrowingService : IBorrowingService
    {
        private readonly IBooksService _booksService;
        private static List<BorrowingRecord> _borrowingRecords = new List<BorrowingRecord>
        {
            new BorrowingRecord 
            { 
                Id = 1, 
                BookId = 3, 
                UserId = "user1", 
                UserName = "John Doe", 
                BorrowedDate = DateTime.Now.AddDays(-10), 
                DueDate = DateTime.Now.AddDays(-3),
                ReturnedDate = null
            },
            new BorrowingRecord 
            { 
                Id = 2, 
                BookId = 1, 
                UserId = "user2", 
                UserName = "Jane Smith", 
                BorrowedDate = DateTime.Now.AddDays(-5), 
                DueDate = DateTime.Now.AddDays(9),
                ReturnedDate = DateTime.Now.AddDays(-1)
            }
        };

        public BorrowingService(IBooksService booksService)
        {
            _booksService = booksService;
        }

        public async Task<BorrowingRecord?> BorrowBookAsync(BorrowBookRequest request)
        {
            await Task.Delay(10); // Simulate async operation
            
            var book = await _booksService.GetBookByIdAsync(request.BookId);
            if (book == null || !book.IsAvailable)
            {
                return null; // Book not found or not available
            }

            var borrowingRecord = new BorrowingRecord
            {
                Id = _borrowingRecords.Count > 0 ? _borrowingRecords.Max(br => br.Id) + 1 : 1,
                BookId = request.BookId,
                UserId = request.UserId,
                UserName = request.UserName,
                BorrowedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14), // 2 weeks borrowing period
                ReturnedDate = null
            };

            _borrowingRecords.Add(borrowingRecord);
            
            // Mark book as unavailable
            book.IsAvailable = false;
            await _booksService.UpdateBookAsync(book.Id, book);

            return borrowingRecord;
        }

        public async Task<bool> ReturnBookAsync(int borrowingId)
        {
            await Task.Delay(10); // Simulate async operation
            
            var borrowingRecord = _borrowingRecords.FirstOrDefault(br => br.Id == borrowingId);
            if (borrowingRecord == null || borrowingRecord.IsReturned)
            {
                return false;
            }

            borrowingRecord.ReturnedDate = DateTime.Now;
            
            // Mark book as available again
            var book = await _booksService.GetBookByIdAsync(borrowingRecord.BookId);
            if (book != null)
            {
                book.IsAvailable = true;
                await _booksService.UpdateBookAsync(book.Id, book);
            }

            return true;
        }

        public async Task<IEnumerable<BorrowingRecord>> GetUserBorrowingsAsync(string userId)
        {
            await Task.Delay(10); // Simulate async operation
            return _borrowingRecords.Where(br => br.UserId == userId);
        }

        public async Task<IEnumerable<BorrowingRecord>> GetOverdueBooksAsync()
        {
            await Task.Delay(10); // Simulate async operation
            return _borrowingRecords.Where(br => br.IsOverdue);
        }
    }
}