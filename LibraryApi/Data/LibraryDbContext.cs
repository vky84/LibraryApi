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
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.MembershipType).HasMaxLength(50);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
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
                
                // Note: UserId is a string reference to User.UserId, not a foreign key to User.Id
                // This maintains backward compatibility with existing data
                // In a future migration, you could add a proper FK relationship
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                
                entity.HasIndex(e => new { e.UserId, e.IsSent });
                entity.HasIndex(e => new { e.IsSent, e.ScheduledFor });
            });

            // Seed User data first (Users must exist before BorrowingRecords reference them)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserId = "user1",
                    UserName = "vky84",
                    Email = "waqas.siddiqui@me.com",
                    FullName = "Waqas Siddiqui",
                    MembershipType = "Standard",
                    JoinedDate = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new User 
                { 
                    Id = 2, 
                    UserId = "user2", 
                    UserName = "Jane Smith",
                    Email = "vky84.com@gmail.com",
                    FullName = "Jane Elizabeth Smith",
                    MembershipType = "Premium",
                    JoinedDate = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new User 
                { 
                    Id = 3, 
                    UserId = "user3", 
                    UserName = "Bob Johnson",
                    Email = "wasi25@student.bth.se",
                    FullName = "Robert James Johnson",
                    MembershipType = "Standard",
                    JoinedDate = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new User
                {
                    Id = 4,
                    UserId = "user4",
                    UserName = "John Doe",
                    Email = "vky84@hotmail.com",
                    FullName = "John Michael Doe",
                    MembershipType = "Standard",
                    JoinedDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );

            // Seed Book data
            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "978-0-7432-7356-5", PublishedDate = DateTime.SpecifyKind(new DateTime(1925, 4, 10), DateTimeKind.Utc), Genre = "Fiction", IsAvailable = true, Description = "A classic American novel" },
                new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0-06-112008-4", PublishedDate = DateTime.SpecifyKind(new DateTime(1960, 7, 11), DateTimeKind.Utc), Genre = "Fiction", IsAvailable = true, Description = "A gripping tale of racial injustice" },
                new Book { Id = 3, Title = "1984", Author = "George Orwell", ISBN = "978-0-452-28423-4", PublishedDate = DateTime.SpecifyKind(new DateTime(1949, 6, 8), DateTimeKind.Utc), Genre = "Dystopian Fiction", IsAvailable = false, Description = "A dystopian social science fiction novel" },
                new Book { Id = 4, Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "978-0-14-143951-8", PublishedDate = DateTime.SpecifyKind(new DateTime(1813, 1, 28), DateTimeKind.Utc), Genre = "Romance", IsAvailable = true, Description = "A romantic novel of manners" },
                new Book { Id = 5, Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "978-0-316-76948-0", PublishedDate = DateTime.SpecifyKind(new DateTime(1951, 7, 16), DateTimeKind.Utc), Genre = "Fiction", IsAvailable = true, Description = "A controversial coming-of-age story" }
            );

            // Seed BorrowingRecord data (now references existing users)
            var baseDate = DateTime.UtcNow.Date.AddDays(-30);
            
            modelBuilder.Entity<BorrowingRecord>().HasData(
                new BorrowingRecord 
                { 
                    Id = 1, 
                    BookId = 3, 
                    UserId = "user1",  // References User.UserId
                    UserName = "John Doe", 
                    BorrowedDate = baseDate.AddDays(-10),
                    DueDate = baseDate.AddDays(4),
                    ReturnedDate = null
                },
                new BorrowingRecord 
                { 
                    Id = 2, 
                    BookId = 1, 
                    UserId = "user2",  // References User.UserId
                    UserName = "Jane Smith", 
                    BorrowedDate = baseDate.AddDays(-5),
                    DueDate = baseDate.AddDays(9),
                    ReturnedDate = baseDate.AddDays(-1)
                }
            );
        }
    }
}