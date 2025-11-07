using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        // Shared tables from LibraryAPI
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
        
        // NotificationService tables
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Book entity (existing from LibraryAPI)
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("Books");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(150);
                entity.Property(e => e.ISBN).HasMaxLength(20);
                entity.Property(e => e.Genre).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasIndex(e => e.ISBN).IsUnique();
            });

            // Configure BorrowingRecord entity (existing from LibraryAPI)
            modelBuilder.Entity<BorrowingRecord>(entity =>
            {
                entity.ToTable("BorrowingRecords");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                
                // Configure relationship with Book
                entity.HasOne<Book>()
                      .WithMany()
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure User entity (NEW)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.MembershipType).HasMaxLength(50);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Notification entity (NEW)
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
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

            // Seed Users data
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    UserId = "user1", 
                    UserName = "John Doe",
                    Email = "john.doe@example.com",
                    FullName = "John Michael Doe",
                    MembershipType = "Standard",
                    JoinedDate = DateTime.UtcNow.AddMonths(-6),
                    IsActive = true
                },
                new User 
                { 
                    Id = 2, 
                    UserId = "user2", 
                    UserName = "Jane Smith",
                    Email = "jane.smith@example.com",
                    FullName = "Jane Elizabeth Smith",
                    MembershipType = "Premium",
                    JoinedDate = DateTime.UtcNow.AddMonths(-12),
                    IsActive = true
                },
                new User 
                { 
                    Id = 3, 
                    UserId = "user3", 
                    UserName = "Bob Johnson",
                    Email = "bob.johnson@example.com",
                    FullName = "Robert James Johnson",
                    MembershipType = "Standard",
                    JoinedDate = DateTime.UtcNow.AddMonths(-3),
                    IsActive = true
                }
            );
        }
    }
}
