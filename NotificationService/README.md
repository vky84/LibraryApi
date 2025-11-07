# Notification Service

A microservice for handling email notifications in the Library Management System. This service runs independently from the LibraryAPI but shares the same PostgreSQL database.

## Features

### Email Notifications
- Manual notification sending via REST API
- Automated notification polling (background service)
- Scheduled notifications
- Retry logic for failed emails

### Notification Types
1. **Book Borrowed** - Confirmation when user borrows a book
2. **Book Returned** - Confirmation when user returns a book
3. **Due Soon Reminder** - 2 days before due date
4. **Overdue Notice** - Daily reminders for overdue books
5. **Welcome Email** - When new user registers
6. **Manual Notification** - Ad-hoc notifications via API

### Background Polling Service
- Polls database every 5 minutes for pending notifications
- Automatically sends overdue reminders
- Handles failed email retries (max 3 attempts)
- Logs all activities

## API Endpoints

### Notifications Controller (`/api/notifications`)

- `POST /api/notifications/send` - Send notification to user immediately
- `GET /api/notifications/user/{userId}` - Get all notifications for a user
- `GET /api/notifications/pending` - Get pending notifications (admin)
- `POST /api/notifications/schedule` - Schedule a notification for future delivery
- `GET /api/notifications/health` - Health check endpoint
- `GET /api/notifications/stats` - Get notification statistics

## Configuration

### Database Connection
The service uses the same PostgreSQL database as LibraryAPI:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres-service;Port=5432;Database=LibraryDb;Username=postgres;Password=postgres"
  }
}
```

### SMTP Configuration
Configure email sending in `appsettings.json`:
```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "library@example.com"
  }
}
```

**Note:** If SMTP is not configured (Host = "smtp.example.com"), emails will be simulated and logged to console.

## Database Tables

### Users
Stores user information for notifications:
- Id, UserId, UserName, Email, FullName
- MembershipType, JoinedDate, IsActive

### Notifications
Tracks all notifications:
- Id, UserId, UserEmail, UserName
- Type, Subject, Message
- BookId, BorrowingRecordId (optional references)
- CreatedAt, ScheduledFor, SentAt
- IsSent, RetryCount, ErrorMessage

## Running the Application

### Locally
```bash
cd NotificationService
dotnet run
```

The service will start on `http://localhost:5000` (or configured port).

### With Docker
```bash
docker build -t vky84/notificationservice:1.0 .
docker run -p 8080:8080 vky84/notificationservice:1.0
```

### With Kubernetes
```bash
# Deploy NotificationService
kubectl apply -f k8s/notificationservice-deployment.yaml
kubectl apply -f k8s/notificationservice-service.yaml

# Check status
kubectl get pods -l app=notificationservice
kubectl logs -f deployment/notificationservice-deployment

# Access Swagger UI
# http://<node-ip>:30082/swagger
```

## Database Migrations

### Create Migration
```bash
dotnet ef migrations add MigrationName --project NotificationService.csproj
```

### Apply Migration
```bash
dotnet ef database update --project NotificationService.csproj
```

**Note:** Migrations are automatically applied on startup.

## Testing the API

### Send a Manual Notification
```bash
curl -X POST http://localhost:30082/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user1",
    "subject": "Test Notification",
    "message": "This is a test email notification!"
  }'
```

### Get User Notifications
```bash
curl http://localhost:30082/api/notifications/user/user1
```

### Check Pending Notifications
```bash
curl http://localhost:30082/api/notifications/pending
```

### Health Check
```bash
curl http://localhost:30082/api/notifications/health
```

## Architecture

### Shared Database Pattern
Both LibraryAPI and NotificationService connect to the same PostgreSQL database:
- **LibraryAPI** manages: Books, BorrowingRecords
- **NotificationService** manages: Users, Notifications
- **Shared tables** are read-only for NotificationService (Books, BorrowingRecords)

### Background Service
`NotificationPollingService` runs as a hosted background service:
1. Starts 30 seconds after application startup
2. Polls database every 5 minutes
3. Processes pending notifications
4. Checks for overdue books and creates reminders

### Email Service
`EmailService` handles actual email delivery:
- Supports real SMTP sending
- Falls back to console logging for development
- Implements retry logic via notification tracking

## Monitoring

### Logs
```bash
# View real-time logs
kubectl logs -f deployment/notificationservice-deployment

# View specific pod logs
kubectl logs <pod-name>
```

### Key Log Messages
- `=== Notification Polling Service Started ===` - Background service initialized
- `? Sent notification {id} to {email}` - Successful email delivery
- `? Failed to send notification {id}` - Email delivery failed
- `=== SIMULATED EMAIL SEND ===` - SMTP not configured, using simulation

## Troubleshooting

### No emails being sent
1. Check SMTP configuration in environment variables
2. Verify `Smtp:Host` is not "smtp.example.com" (simulation mode)
3. Check notification logs: `kubectl logs deployment/notificationservice-deployment`
4. Verify pending notifications: `GET /api/notifications/pending`

### Database connection issues
1. Ensure PostgreSQL is running: `kubectl get pods -l app=postgres`
2. Check connection string in deployment YAML
3. Verify service name: `postgres-service` (not `postgres`)
4. Check logs for connection errors

### Notifications not being created
1. Verify Users table has data: Check if migrations ran successfully
2. Ensure userId exists in Users table
3. Check LibraryAPI is creating borrowing records

## Sample Data

The service seeds 3 users on first run:
- **user1**: john.doe@example.com (John Doe)
- **user2**: jane.smith@example.com (Jane Smith)
- **user3**: bob.johnson@example.com (Bob Johnson)

## Development Notes

### Polling Interval
Adjust polling frequency in `NotificationPollingService.cs`:
```csharp
private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(5);
```

### Retry Limit
Maximum email retry attempts (default: 3):
```csharp
.Where(n => n.RetryCount < 3)
```

### Overdue Check
Daily overdue reminders (prevents duplicate sends within 24 hours):
```csharp
var yesterday = DateTime.UtcNow.AddDays(-1);
```

## Swagger Documentation

Access interactive API documentation:
- Local: `http://localhost:5000/swagger`
- Kubernetes: `http://<node-ip>:30082/swagger`

## Project Structure
```
NotificationService/
??? Controllers/
?   ??? NotificationsController.cs
??? Services/
?   ??? INotificationService.cs
?   ??? NotificationServiceImpl.cs
?   ??? IEmailService.cs
?   ??? EmailService.cs
??? BackgroundServices/
?   ??? NotificationPollingService.cs
??? Models/
?   ??? User.cs
?   ??? Notification.cs
?   ??? Book.cs (read-only)
?   ??? BorrowingRecord.cs (read-only)
?   ??? SendNotificationRequest.cs
??? Data/
?   ??? LibraryDbContext.cs
?   ??? Migrations/
??? k8s/
?   ??? notificationservice-deployment.yaml
?   ??? notificationservice-service.yaml
??? Program.cs
??? appsettings.json
??? appsettings.Development.json
??? Dockerfile
```

## License
MIT

## Author
Library Management System - NotificationService
Version 1.0
