using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    /// <summary>
    /// LibraryDbContext for NotificationService
    /// NOTE: This context only READS from the database.
    /// All table creation, configuration, and seeding is done by LibraryApi.
    /// This is a read-only consumer of the shared database.
    /// </summary>
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        // Shared tables from LibraryAPI (read-only access)
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
        
        // NotificationService tables (created by LibraryApi migrations)
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IMPORTANT: No table configurations here!
            // All table schemas are defined in LibraryApi/Data/LibraryDbContext.cs
            // This ensures single source of truth for database schema.
            
            // EF Core will use conventions or read from the database to understand the schema.
            // If you need to query these tables, EF Core will figure out the structure automatically.
            
            // Optionally, you can add minimal configurations for clarity, but NO HasData() calls
            // These should match EXACTLY what's in LibraryApi/Data/LibraryDbContext.cs
            
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<BorrowingRecord>().ToTable("BorrowingRecords");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Notification>().ToTable("Notifications");
            
            // NO SEED DATA HERE!
            // All seed data is managed by LibraryApi
        }
    }
}
