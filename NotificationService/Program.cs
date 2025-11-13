using Microsoft.EntityFrameworkCore;
using NotificationService.BackgroundServices;
using NotificationService.Data;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS to allow Swagger UI to make requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

// Verify database connectivity (NO MIGRATIONS!)
// LibraryApi is responsible for creating all tables and running migrations
try
{
    Console.WriteLine("=== VERIFYING DATABASE CONNECTIVITY (NOTIFICATION SERVICE) ===");
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Just check if we can connect - DO NOT apply migrations
    logger.LogInformation("Checking database connection...");
    var canConnect = await context.Database.CanConnectAsync();
    
    if (!canConnect)
    {
        logger.LogError("Cannot connect to database!");
        throw new InvalidOperationException("Database connection failed. Make sure LibraryApi has created the database first.");
    }
    
    logger.LogInformation("Database connection successful");
    
    // Verify Users table exists and has data
    try
    {
        var userCount = await context.Users.CountAsync();
        logger.LogInformation("Users in database: {count}", userCount);
        
        if (userCount == 0)
        {
            logger.LogWarning("WARNING: Users table is empty. Make sure LibraryApi has run migrations and seeded data.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "ERROR: Could not read Users table. Make sure LibraryApi has created the schema.");
        logger.LogError("Did you run LibraryApi first? LibraryApi must create all tables before NotificationService can use them.");
        throw;
    }
    
    Console.WriteLine("=== DATABASE VERIFICATION SUCCESSFUL ===");
    Console.WriteLine("NOTE: All database tables are managed by LibraryApi");
    Console.WriteLine("Make sure LibraryApi has run at least once to create the schema.");
    Console.WriteLine("========================================");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "FATAL: Database verification failed.");
    logger.LogError("Exception Type: {type}", ex.GetType().Name);
    logger.LogError("Exception Message: {message}", ex.Message);
    if (ex.InnerException != null)
    {
        logger.LogError("Inner Exception: {innerMessage}", ex.InnerException.Message);
    }
    logger.LogError("");
    logger.LogError("=== IMPORTANT ===");
    logger.LogError("NotificationService does NOT create database tables.");
    logger.LogError("You must run LibraryApi first to create the database schema.");
    logger.LogError("LibraryApi handles all migrations and data seeding.");
    logger.LogError("=================");
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

// Enable CORS - MUST be before UseAuthorization
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("=== NOTIFICATION SERVICE STARTED ===");
Console.WriteLine($"Swagger UI: http://localhost:5089/swagger");
Console.WriteLine($"Swagger UI (HTTPS): https://localhost:7230/swagger");
Console.WriteLine($"Health Check: http://localhost:5089/api/notifications/health");
Console.WriteLine("=====================================");

app.Run();
