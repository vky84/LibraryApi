using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LibraryApi.Data
{
    // This factory is used by the EF Core tools at design-time to create a DbContext
    // without running the application's Program.cs startup logic.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        public LibraryDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("DefaultConnection is not configured in appsettings.");

            optionsBuilder.UseNpgsql(connectionString);

            return new LibraryDbContext(optionsBuilder.Options);
        }
    }
}
