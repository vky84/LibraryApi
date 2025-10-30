using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LibraryApi.Services
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();

            // Try to ensure the database exists on the server. If it was deleted, create it using the
            // postgres maintenance database so MigrateAsync can connect and apply migrations.
            try
            {
                var connectionString = configuration?.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var builder = new NpgsqlConnectionStringBuilder(connectionString);
                    var targetDb = builder.Database;
                    
                    logger.LogInformation("Database connection details - Host: {host}, Port: {port}, Database: {database}, Username: {username}",
                        builder.Host, builder.Port, builder.Database, builder.Username);

                    // If database is empty/null we skip
                    if (!string.IsNullOrEmpty(targetDb))
                    {
                        logger.LogInformation("Checking if database '{db}' exists on host '{host}'...", targetDb, builder.Host);
                        
                        // Connect to the maintenance 'postgres' database to check/create the target DB
                        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
                        {
                            Database = "postgres"
                        };

                        try
                        {
                            await using var adminConn = new NpgsqlConnection(adminBuilder.ConnectionString);
                            logger.LogInformation("Attempting to connect to PostgreSQL at {host}:{port}...", builder.Host, builder.Port);
                            await adminConn.OpenAsync();
                            logger.LogInformation("Successfully connected to PostgreSQL server at {host}:{port}", builder.Host, builder.Port);

                            await using var cmd = adminConn.CreateCommand();
                            cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
                            cmd.Parameters.AddWithValue("name", targetDb);
                            var exists = await cmd.ExecuteScalarAsync();

                            if (exists == null)
                            {
                                logger.LogInformation("Database '{db}' not found. Creating...", targetDb);
                                await using var createCmd = adminConn.CreateCommand();
                                // Use quoted identifier to preserve case if any
                                createCmd.CommandText = $"CREATE DATABASE \"{targetDb}\"";
                                await createCmd.ExecuteNonQueryAsync();
                                logger.LogInformation("Database '{db}' created successfully.", targetDb);
                            }
                            else
                            {
                                logger.LogInformation("Database '{db}' already exists.", targetDb);
                            }
                        }
                        catch (NpgsqlException npgEx)
                        {
                            logger.LogError(npgEx, "PostgreSQL connection error - Host: {host}, Port: {port}, Error Code: {code}", 
                                builder.Host, builder.Port, npgEx.SqlState);
                            logger.LogError("Could not connect to PostgreSQL server. Check:");
                            logger.LogError("  1. PostgreSQL service is running");
                            logger.LogError("  2. Host name '{host}' is correct (should be postgres-service in k8s, not localhost)", builder.Host);
                            logger.LogError("  3. Port {port} is accessible", builder.Port);
                            logger.LogError("  4. Network connectivity between pods");
                            logger.LogError("  5. PostgreSQL is configured to accept connections from this IP");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not NpgsqlException)
            {
                logger.LogWarning(ex, "Could not ensure database exists; continuing and letting Migrate handle it if possible.");
            }

            try
            {
                logger.LogInformation("Applying database migrations...");
                // Apply migrations (creates schema/tables)
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");

                // Seed dummy data if not present
                if (!await context.Books.AnyAsync())
                {
                    logger.LogInformation("Seeding initial book data...");
                    var books = new[]
                    {
                        new Models.Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "978-0-7432-7356-5", PublishedDate = new DateTime(1925, 4, 10), Genre = "Fiction", IsAvailable = true, Description = "A classic American novel" },
                        new Models.Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0-06-112008-4", PublishedDate = new DateTime(1960, 7, 11), Genre = "Fiction", IsAvailable = true, Description = "A gripping tale of racial injustice" },
                        new Models.Book { Id = 3, Title = "1984", Author = "George Orwell", ISBN = "978-0-452-28423-4", PublishedDate = new DateTime(1949, 6, 8), Genre = "Dystopian Fiction", IsAvailable = false, Description = "A dystopian social science fiction novel" },
                        new Models.Book { Id = 4, Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "978-0-14-143951-8", PublishedDate = new DateTime(1813, 1, 28), Genre = "Romance", IsAvailable = true, Description = "A romantic novel of manners" },
                        new Models.Book { Id = 5, Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "978-0-316-76948-0", PublishedDate = new DateTime(1951, 7, 16), Genre = "Fiction", IsAvailable = true, Description = "A controversial coming-of-age story" }
                    };
                    await context.Books.AddRangeAsync(books);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {count} books.", books.Length);
                }
                else
                {
                    logger.LogInformation("Books already exist in database, skipping seed.");
                }

                if (!await context.BorrowingRecords.AnyAsync())
                {
                    logger.LogInformation("Seeding initial borrowing records...");
                    var records = new[]
                    {
                        new Models.BorrowingRecord
                        {
                            Id = 1,
                            BookId = 3,
                            UserId = "user1",
                            UserName = "John Doe",
                            BorrowedDate = DateTime.Now.AddDays(-10),
                            DueDate = DateTime.Now.AddDays(-3),
                            ReturnedDate = null
                        },
                        new Models.BorrowingRecord
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
                    await context.BorrowingRecords.AddRangeAsync(records);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {count} borrowing records.", records.Length);
                }
                else
                {
                    logger.LogInformation("Borrowing records already exist in database, skipping seed.");
                }

                logger.LogInformation("Database initialized successfully with seed data.");
            }
            catch (NpgsqlException npgEx)
            {
                logger.LogError(npgEx, "PostgreSQL error during migration/seeding. Error Code: {code}", npgEx.SqlState);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
    }
}