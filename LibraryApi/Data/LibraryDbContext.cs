using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;

namespace LibraryApi.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(150);
                entity.Property(e => e.ISBN).HasMaxLength(20);
                entity.Property(e => e.Genre).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasIndex(e => e.ISBN).IsUnique();
            });

            // Configure BorrowingRecord entity
            modelBuilder.Entity<BorrowingRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                
                // Configure relationship with Book
                entity.HasOne<Book>()
                      .WithMany()
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "978-0-7432-7356-5", PublishedDate = DateTime.SpecifyKind(new DateTime(1925, 4, 10), DateTimeKind.Utc), Genre = "Fiction", IsAvailable = true, Description = "A classic American novel" },
                new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0-06-112008-4", PublishedDate = DateTime.SpecifyKind(new DateTime(1960, 7, 11), DateTimeKind.Utc), Genre = "Fiction", IsAvailable = true, Description = "A gripping tale of racial injustice" },
                new Book { Id = 3, Title = "1984", Author = "George Orwell", ISBN = "978-0-452-28423-4", PublishedDate = DateTime.SpecifyKind(new DateTime(1949, 6, 8), DateTimeKind.Utc), Genre = "Dystopian Fiction", IsAvailable = false, Description = "A dystopian social science fiction novel" },
                new Book { Id = 4, Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "978-0-14-143951-8", PublishedDate = DateTime.SpecifyKind(new DateTime(1813, 1, 28), DateTimeKind.Utc), Genre = "Romance", IsAvailable = true, Description = "A romantic novel of manners" },
                new Book { Id = 5, Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "978-0-316-76948-0", PublishedDate = DateTime.SpecifyKind(new DateTime(1951, 7, 16), DateTimeKind.Utc), Genre = "Fiction", IsAvailable = true, Description = "A controversial coming-of-age story" }
            );

            // Use fixed UTC dates for seed data instead of DateTime.Now
            var baseDate = DateTime.UtcNow.Date.AddDays(-30); // 30 days ago as base date for consistent seed data
            
            modelBuilder.Entity<BorrowingRecord>().HasData(
                new BorrowingRecord 
                { 
                    Id = 1, 
                    BookId = 3, 
                    UserId = "user1", 
                    UserName = "John Doe", 
                    BorrowedDate = baseDate.AddDays(-10), // Changed from DateTime.Now to baseDate
                    DueDate = baseDate.AddDays(4), // Changed from DateTime.Now to baseDate
                    ReturnedDate = null
                },
                new BorrowingRecord 
                { 
                    Id = 2, 
                    BookId = 1, 
                    UserId = "user2", 
                    UserName = "Jane Smith", 
                    BorrowedDate = baseDate.AddDays(-5), // Changed from DateTime.Now to baseDate
                    DueDate = baseDate.AddDays(9), // Changed from DateTime.Now to baseDate
                    ReturnedDate = baseDate.AddDays(-1) // Changed from DateTime.Now to baseDate
                }
            );
        }
    }
}