using Microsoft.EntityFrameworkCore;
using NotificationService.BackgroundServices;
using NotificationService.Data;
using NotificationService.Services;

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
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// Log connection info (mask password)
var maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
    connectionString, 
    @"Password=([^;]+)", 
    "Password=***");
Console.WriteLine($"=== NOTIFICATION SERVICE - DATABASE CONNECTION INFO ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Connection String: {maskedConnectionString}");
Console.WriteLine($"========================================================");

// Configure Entity Framework with shared database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register custom services
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register background service for polling
builder.Services.AddHostedService<NotificationPollingService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Notification Service API", 
        Version = "v1",
        Description = "Library Notification Service - Handles email notifications for library operations"
    });
});

var app = builder.Build();

// Initialize database (apply migrations and seed data)
try
{
    Console.WriteLine("=== INITIALIZING DATABASE (NOTIFICATION SERVICE) ===");
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Applying database migrations...");
    await context.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied successfully");
    
    // Check if Users table has data
    var userCount = await context.Users.CountAsync();
    logger.LogInformation("Users in database: {count}", userCount);
    
    Console.WriteLine("=== DATABASE INITIALIZED SUCCESSFULLY ===");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "FATAL: Failed to initialize database.");
    logger.LogError("Exception Type: {type}", ex.GetType().Name);
    logger.LogError("Exception Message: {message}", ex.Message);
    if (ex.InnerException != null)
    {
        logger.LogError("Inner Exception: {innerMessage}", ex.InnerException.Message);
    }
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("=== NOTIFICATION SERVICE STARTED ===");
Console.WriteLine($"Swagger UI: http://localhost:{{port}}/swagger");
Console.WriteLine($"Health Check: http://localhost:{{port}}/api/notifications/health");
Console.WriteLine("=====================================");

app.Run();
