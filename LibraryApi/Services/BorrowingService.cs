using LibraryApi.Models;
using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

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
        private readonly LibraryDbContext _context;
        private readonly IBooksService _booksService;

        public BorrowingService(LibraryDbContext context, IBooksService booksService)
        {
            _context = context;
            _booksService = booksService;
        }

        public async Task<BorrowingRecord?> BorrowBookAsync(BorrowBookRequest request)
        {
            var book = await _booksService.GetBookByIdAsync(request.BookId);
            if (book == null || !book.IsAvailable)
            {
                return null; // Book not found or not available
            }

            var borrowingRecord = new BorrowingRecord
            {
                BookId = request.BookId,
                UserId = request.UserId,
                UserName = request.UserName,
                BorrowedDate = DateTime.UtcNow, // Changed from DateTime.Now to DateTime.UtcNow
                DueDate = DateTime.UtcNow.AddDays(14), // Changed from DateTime.Now to DateTime.UtcNow
                ReturnedDate = null
            };

            _context.BorrowingRecords.Add(borrowingRecord);
            
            // Mark book as unavailable
            book.IsAvailable = false;
            await _context.SaveChangesAsync();

            return borrowingRecord;
        }

        public async Task<bool> ReturnBookAsync(int borrowingId)
        {
            var borrowingRecord = await _context.BorrowingRecords.FindAsync(borrowingId);
            if (borrowingRecord == null || borrowingRecord.IsReturned)
            {
                return false;
            }

            borrowingRecord.ReturnedDate = DateTime.UtcNow; // Changed from DateTime.Now to DateTime.UtcNow
            
            // Mark book as available again
            var book = await _booksService.GetBookByIdAsync(borrowingRecord.BookId);
            if (book != null)
            {
                book.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BorrowingRecord>> GetUserBorrowingsAsync(string userId)
        {
            return await _context.BorrowingRecords
                .Where(br => br.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetOverdueBooksAsync()
        {
            return await _context.BorrowingRecords
                .Where(br => !br.IsReturned && DateTime.UtcNow > br.DueDate) // Changed from DateTime.Now to DateTime.UtcNow
                .ToListAsync();
        }
    }
}