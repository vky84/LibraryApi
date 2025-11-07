# NotificationService - Setup and Testing Guide

## ? Project Successfully Created!

The NotificationService has been successfully scaffolded as a separate microservice that shares the same PostgreSQL database with LibraryAPI.

---

## ?? Project Structure

```
LibraryApi/ (Solution)
??? LibraryApi/                          # Main Library API
?   ??? Controllers/
?   ??? Services/
?   ??? Models/
?   ??? Data/
?   ??? k8s/
??? NotificationService/                 # NEW Notification Microservice
    ??? Controllers/
    ?   ??? NotificationsController.cs   # REST API endpoints
    ??? Services/
    ?   ??? NotificationServiceImpl.cs   # Business logic
    ?   ??? EmailService.cs              # Email sending
    ??? BackgroundServices/
    ?   ??? NotificationPollingService.cs # DB polling (every 5 min)
    ??? Models/
    ?   ??? User.cs                      # NEW table
    ?   ??? Notification.cs              # NEW table
    ?   ??? Book.cs                      # Shared (read-only)
    ?   ??? BorrowingRecord.cs           # Shared (read-only)
    ??? Data/
    ?   ??? LibraryDbContext.cs          # Shared DB context
    ?   ??? Migrations/
    ?       ??? AddUsersAndNotifications # Migration for new tables
    ??? k8s/
    ?   ??? notificationservice-deployment.yaml
    ?   ??? notificationservice-service.yaml
    ??? Dockerfile
    ??? README.md
    ??? NotificationService.http         # API testing file

```

---

## ??? Database Schema

### Shared Database: `LibraryDb`

**Existing Tables (managed by LibraryAPI):**
- `Books` - Book catalog
- `BorrowingRecords` - Borrowing history

**New Tables (managed by NotificationService):**
- `Users` - User information with emails
- `Notifications` - Notification queue and history

### Database Sharing Pattern
```
???????????????????????????????????????????????????
?         PostgreSQL (LibraryDb)                  ?
???????????????????????????????????????????????????
?  Books             ??? LibraryAPI (Read/Write)  ?
?                    ??? NotificationService (Read)?
?                                                  ?
?  BorrowingRecords  ??? LibraryAPI (Read/Write)  ?
?                    ??? NotificationService (Read)?
?                                                  ?
?  Users             ??? NotificationService (RW) ?
?  Notifications     ??? NotificationService (RW) ?
???????????????????????????????????????????????????
```

---

## ?? Running the Services

### Option 1: Run Locally (Development)

**Terminal 1 - Start LibraryAPI:**
```bash
cd LibraryApi
dotnet run
# Runs on http://localhost:5262
```

**Terminal 2 - Start NotificationService:**
```bash
cd NotificationService
dotnet run
# Runs on http://localhost:5089
```

**Terminal 3 - Start PostgreSQL (if not running):**
```bash
# Via Docker
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Or use your local PostgreSQL installation
```

### Option 2: Run with Kubernetes

**Deploy all services:**
```bash
# 1. Deploy PostgreSQL
kubectl apply -f k8s/postgres-deployment.yaml

# 2. Deploy LibraryAPI
kubectl apply -f k8s/libraryapi-deployment.yaml
kubectl apply -f k8s/libraryapi-service.yaml

# 3. Deploy NotificationService
kubectl apply -f k8s/notificationservice-deployment.yaml
kubectl apply -f k8s/notificationservice-service.yaml

# 4. Check status
kubectl get pods
kubectl get services
```

**Access endpoints:**
- **LibraryAPI**: http://localhost:30081/swagger
- **NotificationService**: http://localhost:30082/swagger

---

## ?? Database Migrations

### Apply Migrations (First Time Setup)

**NotificationService creates Users and Notifications tables:**
```bash
cd NotificationService
dotnet ef database update
```

The migrations are **automatically applied on startup** when the service runs.

### Verify Database Tables

```sql
-- Connect to PostgreSQL
psql -U postgres -d LibraryDb

-- List all tables
\dt

-- Should show:
-- Books
-- BorrowingRecords
-- Users
-- Notifications
-- __EFMigrationsHistory

-- Check Users
SELECT * FROM "Users";

-- Check Notifications
SELECT * FROM "Notifications";
```

---

## ?? Email Configuration

### Development Mode (Simulated Emails)

By default, emails are **logged to console** (not actually sent):
```json
{
  "Smtp": {
    "Host": "smtp.example.com",  // Triggers simulation mode
    "Port": "587",
    "Username": "your-email@example.com",
    "Password": "your-password",
    "FromEmail": "library-notifications@example.com"
  }
}
```

**Console Output:**
```
=== SIMULATED EMAIL SEND ===
To: john.doe@example.com
Subject: Test Notification
Body: This is a test email!
============================
```

### Production Mode (Real SMTP)

Update `appsettings.json` or environment variables:

**For Gmail:**
```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",  // Get from Google Account settings
    "FromEmail": "library@yourdomain.com"
  }
}
```

**For Kubernetes:**
```yaml
env:
- name: Smtp__Host
  value: "smtp.gmail.com"
- name: Smtp__Port
  value: "587"
- name: Smtp__Username
  value: "your-email@gmail.com"
- name: Smtp__Password
  value: "your-app-password"
- name: Smtp__FromEmail
  value: "library@yourdomain.com"
```

---

## ?? Testing the APIs

### Using NotificationService.http file

**Open in Visual Studio / Rider:**
1. Open `NotificationService/NotificationService.http`
2. Click "Run" next to any request

**Available Endpoints:**

? **Health Check**
```http
GET http://localhost:5089/api/notifications/health
```

? **Send Notification to User**
```http
POST http://localhost:5089/api/notifications/send
Content-Type: application/json

{
  "userId": "user1",
  "subject": "Test Notification",
  "message": "This is a test!"
}
```

? **Get User's Notifications**
```http
GET http://localhost:5089/api/notifications/user/user1
```

? **Get Pending Notifications**
```http
GET http://localhost:5089/api/notifications/pending
```

? **Get Statistics**
```http
GET http://localhost:5089/api/notifications/stats
```

### Using Swagger UI

**Local:**
- LibraryAPI: http://localhost:5262/swagger
- NotificationService: http://localhost:5089/swagger

**Kubernetes:**
- LibraryAPI: http://localhost:30081/swagger
- NotificationService: http://localhost:30082/swagger

---

## ?? Background Polling Service

### How It Works

The `NotificationPollingService` runs automatically when NotificationService starts:

1. **Starts:** 30 seconds after application startup
2. **Polls:** Every 5 minutes
3. **Actions:**
   - Sends pending notifications
   - Checks for overdue books
   - Creates overdue reminders

### Monitor Polling Activity

**View logs:**
```bash
# Local
dotnet run  # Watch console output

# Kubernetes
kubectl logs -f deployment/notificationservice-deployment
```

**Expected Log Output:**
```
=== Notification Polling Service Started ===
Polling interval: 5 minutes
Starting notification polling cycle...
Found 3 pending notifications to process
? Sent notification 1 to john.doe@example.com: Test Notification
? Sent notification 2 to jane.smith@example.com: Welcome Email
Processed 2 notifications
Checking for overdue book reminders...
Found 1 overdue borrowing records
Created overdue notice for user user1, book: 1984
Next polling cycle in 5 minutes
```

### Adjust Polling Frequency

Edit `BackgroundServices/NotificationPollingService.cs`:
```csharp
// Change from 5 minutes to 1 minute for faster testing
private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);
```

---

## ?? Complete Testing Workflow

### 1. Setup Database and Services

```bash
# Terminal 1: Start PostgreSQL
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Terminal 2: Start LibraryAPI
cd LibraryApi
dotnet run

# Terminal 3: Start NotificationService
cd NotificationService
dotnet run
```

### 2. Verify Database Seeding

```bash
# Check if users were created
curl http://localhost:5089/api/notifications/stats
# Should show seeded users: user1, user2, user3
```

### 3. Send Test Notification

```bash
curl -X POST http://localhost:5089/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user1",
    "subject": "Test Notification",
    "message": "Hello from NotificationService!"
  }'
```

**Expected Response:**
```json
{
  "message": "Notification sent successfully",
  "userId": "user1",
  "subject": "Test Notification"
}
```

**Check Console for Simulated Email:**
```
=== SIMULATED EMAIL SEND ===
To: john.doe@example.com
Subject: Test Notification
Body: Hello from NotificationService!
============================
```

### 4. Verify Notification was Saved

```bash
curl http://localhost:5089/api/notifications/user/user1
```

**Expected Response:**
```json
{
  "userId": "user1",
  "count": 1,
  "notifications": [
    {
      "id": 1,
      "userId": "user1",
      "userEmail": "john.doe@example.com",
      "subject": "Test Notification",
      "message": "Hello from NotificationService!",
      "isSent": true,
      "sentAt": "2025-01-08T..."
    }
  ]
}
```

### 5. Test Integration with LibraryAPI

**Borrow a book (LibraryAPI):**
```bash
curl -X POST http://localhost:5262/api/borrowing/borrow \
  -H "Content-Type: application/json" \
  -d '{
    "bookId": 1,
    "userId": "user1",
    "userName": "John Doe"
  }'
```

**Check for overdue books (after waiting 5 minutes for polling):**
```bash
# The background service will automatically create overdue notifications
curl http://localhost:5089/api/notifications/pending
```

---

## ?? Docker Deployment

### Build Docker Images

**NotificationService:**
```bash
cd NotificationService
docker build -t vky84/notificationservice:1.0 .
docker push vky84/notificationservice:1.0
```

**LibraryAPI:**
```bash
cd LibraryApi
docker build -t vky84/libraryapi:1.0 .
docker push vky84/libraryapi:1.0
```

### Run with Docker Compose (Optional)

Create `docker-compose.yml`:
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: LibraryDb
    ports:
      - "5432:5432"
  
  libraryapi:
    image: vky84/libraryapi:1.0
    ports:
      - "5262:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=LibraryDb;Username=postgres;Password=postgres"
    depends_on:
      - postgres
  
  notificationservice:
    image: vky84/notificationservice:1.0
    ports:
      - "5089:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=LibraryDb;Username=postgres;Password=postgres"
    depends_on:
      - postgres
```

```bash
docker-compose up
```

---

## ?? Troubleshooting

### Issue: Notifications not being sent

**Check:**
1. Is NotificationService running? `kubectl get pods -l app=notificationservice`
2. Check logs: `kubectl logs deployment/notificationservice-deployment`
3. Verify SMTP config: Should see "SIMULATED EMAIL SEND" in logs if SMTP not configured
4. Check pending notifications: `GET /api/notifications/pending`

### Issue: User not found

**Check:**
1. Verify users table has data: `curl http://localhost:5089/api/notifications/stats`
2. Ensure userId exists: "user1", "user2", "user3" are seeded by default
3. Check database: `SELECT * FROM "Users";`

### Issue: Database connection failed

**Check:**
1. PostgreSQL is running: `kubectl get pods -l app=postgres`
2. Connection string is correct
3. Service name in K8s: `postgres-service` (not `postgres`)
4. Check logs for detailed error

### Issue: Background polling not working

**Check:**
1. Look for "Notification Polling Service Started" in logs
2. Wait at least 5 minutes for first poll cycle
3. Check interval setting in `NotificationPollingService.cs`

---

## ?? Monitoring and Logs

### View Real-Time Logs

**Local:**
```bash
# LibraryAPI
cd LibraryApi && dotnet run

# NotificationService
cd NotificationService && dotnet run
```

**Kubernetes:**
```bash
# LibraryAPI logs
kubectl logs -f deployment/libraryapi-deployment

# NotificationService logs
kubectl logs -f deployment/notificationservice-deployment

# PostgreSQL logs
kubectl logs -f deployment/postgres
```

### Key Log Messages to Watch

? **Startup:**
```
=== NOTIFICATION SERVICE - DATABASE CONNECTION INFO ===
=== INITIALIZING DATABASE (NOTIFICATION SERVICE) ===
=== DATABASE INITIALIZED SUCCESSFULLY ===
=== Notification Polling Service Started ===
```

? **Email Sent:**
```
? Sent notification 1 to john.doe@example.com: Test Subject
```

? **Email Failed:**
```
? Failed to send notification 1 (retry 1/3)
```

---

## ?? Academic Project Notes

### Demonstrates Microservices Concepts

1. ? **Two independent services** (LibraryAPI + NotificationService)
2. ? **Shared database pattern** (both use LibraryDb)
3. ? **Service separation** (different business domains)
4. ? **Independent deployment** (separate Docker images & K8s deployments)
5. ? **Background processing** (polling service)
6. ? **REST API** (synchronous communication)
7. ? **Kubernetes orchestration** (separate pods, services)

### For Presentation

- **Architecture diagram** shows clear separation
- **Different ports:** LibraryAPI (30081) vs NotificationService (30082)
- **Shared database** reduces complexity while maintaining service independence
- **Polling pattern** simpler than event-driven (no Kafka/RabbitMQ needed)

---

## ? Summary Checklist

- [x] NotificationService project created
- [x] DbContext configured for shared database
- [x] Users and Notifications tables added
- [x] Email service with SMTP support
- [x] Background polling service (5-minute interval)
- [x] REST API endpoints for manual notifications
- [x] Swagger documentation
- [x] Docker support (Dockerfile)
- [x] Kubernetes deployment files
- [x] Testing file (NotificationService.http)
- [x] Comprehensive README
- [x] Database migrations

---

## ?? Next Steps

1. **Test Locally:**
   - Run both services
   - Send test notification
   - Verify email simulation in console

2. **Deploy to Kubernetes:**
   - Build Docker images
   - Apply K8s manifests
   - Test via NodePort

3. **Configure Real SMTP:**
   - Get Gmail app password
   - Update SMTP settings
   - Test real email sending

4. **Monitor Polling:**
   - Wait 5 minutes
   - Check logs for polling activity
   - Verify overdue notifications created

---

**Project Status:** ? **READY FOR TESTING AND DEPLOYMENT**

For detailed API documentation, see: [NotificationService/README.md](../NotificationService/README.md)
