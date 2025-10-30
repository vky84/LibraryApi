using Microsoft.AspNetCore.Mvc;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;

        public BorrowingController(IBorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        /// <summary>
        /// Borrow a book
        /// </summary>
        [HttpPost("borrow")]
        public async Task<ActionResult<BorrowingRecord>> BorrowBook([FromBody] BorrowBookRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var borrowingRecord = await _borrowingService.BorrowBookAsync(request);
            if (borrowingRecord == null)
            {
                return BadRequest("Book is not available for borrowing or does not exist.");
            }

            return CreatedAtAction(nameof(GetUserBorrowings), new { userId = request.UserId }, borrowingRecord);
        }

        /// <summary>
        /// Return a borrowed book
        /// </summary>
        [HttpPost("return/{borrowingId}")]
        public async Task<ActionResult> ReturnBook(int borrowingId)
        {
            var returned = await _borrowingService.ReturnBookAsync(borrowingId);
            if (!returned)
            {
                return BadRequest("Invalid borrowing ID or book already returned.");
            }

            return Ok(new { message = "Book returned successfully." });
        }

        /// <summary>
        /// Get all books borrowed by a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BorrowingRecord>>> GetUserBorrowings(string userId)
        {
            var borrowings = await _borrowingService.GetUserBorrowingsAsync(userId);
            return Ok(borrowings);
        }

        /// <summary>
        /// Get all overdue books
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<BorrowingRecord>>> GetOverdueBooks()
        {
            var overdueBooks = await _borrowingService.GetOverdueBooksAsync();
            return Ok(overdueBooks);
        }
    }
}