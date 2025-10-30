using Microsoft.AspNetCore.Mvc;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksService _booksService;

        public BooksController(IBooksService booksService)
        {
            _booksService = booksService;
        }

        /// <summary>
        /// Get all books
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = await _booksService.GetAllBooksAsync();
            return Ok(books);
        }

        /// <summary>
        /// Get available books only
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Book>>> GetAvailableBooks()
        {
            var books = await _booksService.GetAvailableBooksAsync();
            return Ok(books);
        }

        /// <summary>
        /// Get book by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _booksService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }
            return Ok(book);
        }

        /// <summary>
        /// Add a new book
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBook = await _booksService.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }

        /// <summary>
        /// Update an existing book
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Book>> UpdateBook(int id, [FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBook = await _booksService.UpdateBookAsync(id, book);
            if (updatedBook == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            return Ok(updatedBook);
        }

        /// <summary>
        /// Delete a book
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var deleted = await _booksService.DeleteBookAsync(id);
            if (!deleted)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            return NoContent();
        }
    }
}