using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Ensure database exists
                await context.Database.EnsureCreatedAsync();
                
                // Check if data already exists
                if (await context.Books.AnyAsync())
                {
                    logger.LogInformation("Database already contains data. Skipping seed.");
                    return;
                }

                logger.LogInformation("Database initialized successfully with seed data.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
    }
}