using LibraryApi.Models;

namespace LibraryApi.Services
{
    public interface IBooksService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book> AddBookAsync(Book book);
        Task<Book?> UpdateBookAsync(int id, Book book);
        Task<bool> DeleteBookAsync(int id);
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
    }

    public class BooksService : IBooksService
    {
        private static List<Book> _books = new List<Book>
        {
            new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "978-0-7432-7356-5", PublishedDate = new DateTime(1925, 4, 10), Genre = "Fiction", IsAvailable = true, Description = "A classic American novel" },
            new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0-06-112008-4", PublishedDate = new DateTime(1960, 7, 11), Genre = "Fiction", IsAvailable = true, Description = "A gripping tale of racial injustice" },
            new Book { Id = 3, Title = "1984", Author = "George Orwell", ISBN = "978-0-452-28423-4", PublishedDate = new DateTime(1949, 6, 8), Genre = "Dystopian Fiction", IsAvailable = false, Description = "A dystopian social science fiction novel" },
            new Book { Id = 4, Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "978-0-14-143951-8", PublishedDate = new DateTime(1813, 1, 28), Genre = "Romance", IsAvailable = true, Description = "A romantic novel of manners" },
            new Book { Id = 5, Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "978-0-316-76948-0", PublishedDate = new DateTime(1951, 7, 16), Genre = "Fiction", IsAvailable = true, Description = "A controversial coming-of-age story" }
        };

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            await Task.Delay(10); // Simulate async operation
            return _books;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            await Task.Delay(10); // Simulate async operation
            return _books.FirstOrDefault(b => b.Id == id);
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            await Task.Delay(10); // Simulate async operation
            book.Id = _books.Max(b => b.Id) + 1;
            _books.Add(book);
            return book;
        }

        public async Task<Book?> UpdateBookAsync(int id, Book book)
        {
            await Task.Delay(10); // Simulate async operation
            var existingBook = _books.FirstOrDefault(b => b.Id == id);
            if (existingBook == null) return null;

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.ISBN = book.ISBN;
            existingBook.PublishedDate = book.PublishedDate;
            existingBook.Genre = book.Genre;
            existingBook.IsAvailable = book.IsAvailable;
            existingBook.Description = book.Description;

            return existingBook;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            await Task.Delay(10); // Simulate async operation
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null) return false;

            _books.Remove(book);
            return true;
        }

        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            await Task.Delay(10); // Simulate async operation
            return _books.Where(b => b.IsAvailable);
        }
    }
}