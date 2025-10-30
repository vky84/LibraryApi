using LibraryApi.Services;
using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Log the connection string being used (mask password for security)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    builder.Logging.AddConsole();
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogError("CRITICAL: No connection string 'DefaultConnection' found in configuration!");
    logger.LogError("Environment: {env}", builder.Environment.EnvironmentName);
    logger.LogError("Available connection strings: {keys}", 
        string.Join(", ", builder.Configuration.GetSection("ConnectionStrings").GetChildren().Select(x => x.Key)));
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// Log connection info (mask password)
var maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
    connectionString, 
    @"Password=([^;]+)", 
    "Password=***");
Console.WriteLine($"=== DATABASE CONNECTION INFO ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Connection String: {maskedConnectionString}");
Console.WriteLine($"================================");

// Configure Entity Framework
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register custom services
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Library API", Version = "v1" });
});

var app = builder.Build();

// Initialize database (apply migrations and seed data)
// Use the DatabaseInitializer to apply migrations and seed dummy data when needed.
try
{
    Console.WriteLine("=== INITIALIZING DATABASE ===");
    await LibraryApi.Services.DatabaseInitializer.InitializeAsync(app.Services);
    Console.WriteLine("=== DATABASE INITIALIZED SUCCESSFULLY ===");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "FATAL: Failed to initialize database. Connection string: {connectionString}", maskedConnectionString);
    logger.LogError("Exception Type: {type}", ex.GetType().Name);
    logger.LogError("Exception Message: {message}", ex.Message);
    if (ex.InnerException != null)
    {
        logger.LogError("Inner Exception: {innerMessage}", ex.InnerException.Message);
    }
    
    // Log environment variables for debugging (in non-production environments)
    if (!app.Environment.IsProduction())
    {
        logger.LogError("Environment Variables related to ConnectionStrings:");
        foreach (var envVar in Environment.GetEnvironmentVariables().Keys.Cast<string>()
            .Where(k => k.Contains("Connection", StringComparison.OrdinalIgnoreCase))
            .OrderBy(k => k))
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            var maskedValue = value != null ? System.Text.RegularExpressions.Regex.Replace(value, @"Password=([^;]+)", "Password=***") : "null";
            logger.LogError("  {key} = {value}", envVar, maskedValue);
        }
    }
    
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
