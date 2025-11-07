# Database Migration Strategy - LibraryApi & NotificationService

## ?? Summary of Changes

You're absolutely right! We need to:
1. **Create a separate Users table** (instead of storing user info in BorrowingRecords)
2. **Add proper relationships** between tables
3. **Centralize migrations** in one location (LibraryApi)

---

## ?? Solution: Single Source of Truth for Migrations

### Recommended Approach: **LibraryApi Manages ALL Migrations**

```
C:\Personals\LibraryApi\
??? LibraryApi\
?   ??? Data\
?       ??? LibraryDbContext.cs          ? Master DbContext
?       ??? Migrations\                  ? ALL migrations here
?           ??? YYYYMMDDHHMMSS_MigrationName.cs
?
??? NotificationService\
    ??? Data\
        ??? LibraryDbContext.cs          ? Same config, NO migrations folder
```

**Why this works:**
- ? Single source of truth for database schema
- ? No migration conflicts between projects
- ? Both services can call `MigrateAsync()` safely (idempotent)
- ? Clear ownership: LibraryApi owns schema, NotificationService uses it

---

## ?? What I've Done

### 1. Created User Model in LibraryApi

**File:** `LibraryApi/Models/User.cs`

```csharp
public class User
{
    public int Id { get; set; }              // Primary key
    public string UserId { get; set; }       // Unique identifier (user1, user2)
    public string UserName { get; set; }     // Display name
    public string Email { get; set; }        // Email for notifications
    public string FullName { get; set; }     // Full legal name
    public string MembershipType { get; set; }  // Standard, Premium
    public DateTime JoinedDate { get; set; }
    public bool IsActive { get; set; }
}
```

### 2. Created Notification Model in LibraryApi

**File:** `LibraryApi/Models/Notification.cs`

```csharp
public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }       // Links to User.UserId
    public string UserEmail { get; set; }
    public string UserName { get; set; }
    public NotificationType Type { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public int? BookId { get; set; }         // Optional: related book
    public int? BorrowingRecordId { get; set; }  // Optional: related borrowing
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### 3. Updated LibraryApi DbContext

**File:** `LibraryApi/Data/LibraryDbContext.cs`

Now includes:
```csharp
public DbSet<Book> Books { get; set; }
public DbSet<User> Users { get; set; }              // NEW
public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
public DbSet<Notification> Notifications { get; set; }  // NEW
```

**Key Changes:**
- ? Added Users table with unique constraints on UserId and Email
- ? Added Notifications table with proper indexes
- ? Seeded 3 users: user1, user2, user3 with emails
- ? BorrowingRecords now reference Users (via string UserId field)

---

## ?? New Database Schema

```sql
-- Users table (NEW)
CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(50) NOT NULL UNIQUE,      -- user1, user2, user3
    "UserName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(200) NOT NULL UNIQUE,      -- john.doe@example.com
    "FullName" VARCHAR(150),
    "MembershipType" VARCHAR(50),
    "JoinedDate" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL
);

-- Books table (existing)
CREATE TABLE "Books" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Author" VARCHAR(150) NOT NULL,
    "ISBN" VARCHAR(20) UNIQUE,
    ...
);

-- BorrowingRecords table (existing, but now references Users)
CREATE TABLE "BorrowingRecords" (
    "Id" SERIAL PRIMARY KEY,
    "BookId" INT NOT NULL,
    "UserId" VARCHAR(50) NOT NULL,       -- References Users.UserId
    "UserName" VARCHAR(100) NOT NULL,    -- Denormalized for convenience
    "BorrowedDate" TIMESTAMP NOT NULL,
    "DueDate" TIMESTAMP NOT NULL,
    "ReturnedDate" TIMESTAMP NULL,
    FOREIGN KEY ("BookId") REFERENCES "Books"("Id")
    -- Note: UserId is a string reference, not a FK to Users.Id
);

-- Notifications table (NEW)
CREATE TABLE "Notifications" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(50) NOT NULL,       -- References Users.UserId
    "UserEmail" VARCHAR(200) NOT NULL,
    "UserName" VARCHAR(100),
    "Type" INT NOT NULL,                 -- Enum: BookBorrowed, DueSoonReminder, etc.
    "Subject" VARCHAR(200) NOT NULL,
    "Message" VARCHAR(2000) NOT NULL,
    "BookId" INT NULL,
    "BorrowingRecordId" INT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "ScheduledFor" TIMESTAMP NULL,
    "IsSent" BOOLEAN NOT NULL,
    "SentAt" TIMESTAMP NULL,
    "RetryCount" INT NOT NULL,
    "ErrorMessage" VARCHAR(500) NULL
);

-- Indexes
CREATE INDEX idx_user_userid ON "Users"("UserId");
CREATE INDEX idx_user_email ON "Users"("Email");
CREATE INDEX idx_notification_userid_sent ON "Notifications"("UserId", "IsSent");
CREATE INDEX idx_notification_sent_scheduled ON "Notifications"("IsSent", "ScheduledFor");
```

---

## ?? How to Apply the Migration

### Step 1: Drop Existing Database (Fresh Start)

**?? WARNING: This deletes all data!**

```powershell
# Connect to PostgreSQL
psql -U postgres

# Drop and recreate database
DROP DATABASE IF EXISTS "LibraryDb";
CREATE DATABASE "LibraryDb";
\q
```

### Step 2: Create Migration in LibraryApi

```powershell
cd C:\Personals\LibraryApi\LibraryApi

# Create migration
dotnet ef migrations add InitialCreateWithUsers

# This creates:
# Data/Migrations/YYYYMMDDHHMMSS_InitialCreateWithUsers.cs
# Data/Migrations/YYYYMMDDHHMMSS_InitialCreateWithUsers.Designer.cs
# Data/Migrations/LibraryDbContextModelSnapshot.cs
```

### Step 3: Apply Migration

```powershell
# Option 1: Manual apply
dotnet ef database update

# Option 2: Run app (auto-migration on startup)
dotnet run
```

### Step 4: Verify Tables Exist

```powershell
psql -U postgres -d LibraryDb

# List tables
\dt

# Should show:
#  Schema |       Name            | Type  |  Owner
# --------+-----------------------+-------+----------
#  public | Books                 | table | postgres
#  public | Users                 | table | postgres  ? NEW
#  public | BorrowingRecords      | table | postgres
#  public | Notifications         | table | postgres  ? NEW
#  public | __EFMigrationsHistory | table | postgres

# Check users data
SELECT * FROM "Users";
# Should show: user1, user2, user3 with emails
```

### Step 5: Run NotificationService

```powershell
cd C:\Personals\LibraryApi\NotificationService
dotnet run

# Check console output:
# - "Applying database migrations..." 
# - "Users in database: 3"  ? Should see this!
# - "DATABASE INITIALIZED SUCCESSFULLY"
```

---

## ?? Update NotificationService DbContext

Since NotificationService will use the same database schema, update its DbContext to match:

**File:** `NotificationService/Data/LibraryDbContext.cs`

**Change from:**
```csharp
// Seed data here (WRONG - causes conflicts)
modelBuilder.Entity<User>().HasData(...);
```

**To:**
```csharp
// NO seed data in NotificationService!
// LibraryApi handles all seeding
```

**Keep the same table configurations** but **remove all `.HasData()` calls**.

---

## ?? Where to Place Shared Migrations?

You asked about placing migrations in a common location. Here are your options:

### Option 1: LibraryApi Owns Migrations (Recommended) ?

```
C:\Personals\LibraryApi\
??? LibraryApi\
?   ??? Data\Migrations\     ? ALL migrations here
??? NotificationService\
    ??? (NO Migrations folder)
```

**Pros:**
- ? Simple and clear
- ? Single source of truth
- ? LibraryApi is the "main" service

**Cons:**
- ?? NotificationService depends on LibraryApi for schema

### Option 2: Shared Data Library (Advanced)

```
C:\Personals\LibraryApi\
??? LibraryApi.Data\           ? NEW shared project
?   ??? Models\
?   ?   ??? User.cs
?   ?   ??? Book.cs
?   ?   ??? BorrowingRecord.cs
?   ?   ??? Notification.cs
?   ??? LibraryDbContext.cs
?   ??? Migrations\            ? Migrations here
??? LibraryApi\
?   ??? (references LibraryApi.Data)
??? NotificationService\
    ??? (references LibraryApi.Data)
```

**Pros:**
- ? True separation of concerns
- ? Both projects reference shared library
- ? No duplication

**Cons:**
- ?? More complex project structure
- ?? Requires restructuring existing code

### Option 3: Solution-Level Migrations Folder

```
C:\Personals\LibraryApi\
??? Migrations\                ? Solution-level migrations
?   ??? LibraryDb\
?       ??? YYYYMMDDHHMMSS_MigrationName.cs
??? LibraryApi\
??? NotificationService\
```

**Pros:**
- ? Clear separation from application code
- ? Easy to find all migrations

**Cons:**
- ?? Requires custom EF Core configuration
- ?? Less common pattern

---

## ? My Recommendation

**For your academic project:** Stick with **Option 1** (LibraryApi owns migrations).

**Reasons:**
1. Simplest to understand and explain
2. Follows microservices pattern (one service owns the schema)
3. Easy to demonstrate in presentations
4. No complex restructuring needed

**In production:** Consider **Option 2** (shared data library) for better separation.

---

## ??? Complete Setup Script

Here's the complete script to reset and apply the new schema:

```powershell
# ===== STEP 1: Backup existing data (if needed) =====
# psql -U postgres -d LibraryDb -c "COPY (SELECT * FROM \"BorrowingRecords\") TO 'C:\\backup_borrowing.csv' CSV HEADER;"

# ===== STEP 2: Drop and recreate database =====
psql -U postgres -c "DROP DATABASE IF EXISTS \"LibraryDb\";"
psql -U postgres -c "CREATE DATABASE \"LibraryDb\";"

# ===== STEP 3: Remove old migrations from LibraryApi =====
cd C:\Personals\LibraryApi\LibraryApi
Remove-Item -Path "Data\Migrations\*" -Recurse -Force -ErrorAction SilentlyContinue

# ===== STEP 4: Create new comprehensive migration =====
dotnet ef migrations add InitialCreateWithUsers

# ===== STEP 5: Apply migration =====
dotnet ef database update

# ===== STEP 6: Verify =====
psql -U postgres -d LibraryDb -c "\dt"
psql -U postgres -d LibraryDb -c "SELECT * FROM \"Users\";"

# ===== STEP 7: Test LibraryApi =====
dotnet run
# Open browser: http://localhost:5262/swagger

# ===== STEP 8: Test NotificationService =====
cd ..\NotificationService
dotnet run
# Open browser: http://localhost:5089/swagger

# ===== STEP 9: Test notification sending =====
# Use NotificationService.http file to send test notification
```

---

## ?? Benefits of This Approach

### For LibraryApi
- ? Can query Users directly
- ? Proper data relationships
- ? Can validate UserId exists before creating BorrowingRecords

### For NotificationService
- ? Has access to Users table with emails
- ? Can send notifications to real users
- ? No need to manage its own migrations

### For Your Academic Project
- ? Demonstrates proper database design
- ? Shows microservice coordination
- ? Clear separation of responsibilities
- ? Easy to explain architecture decisions

---

## ?? Next Steps

1. **Run the setup script above** to recreate the database
2. **Test LibraryApi** to ensure books and borrowing work
3. **Test NotificationService** to ensure user data is available
4. **Send a test notification** using the `/api/notifications/send` endpoint
5. **Update your architecture documentation** to reflect the Users table

---

## ?? Troubleshooting

### "dotnet-ef not found"

```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef --version 8.0.0

# Or update if already installed
dotnet tool update --global dotnet-ef --version 8.0.0
```

### "Migration already exists"

```powershell
# Remove all migrations
Remove-Item -Path "Data\Migrations\*" -Recurse -Force

# Create fresh migration
dotnet ef migrations add InitialCreateWithUsers
```

### "Table 'Users' doesn't exist" in NotificationService

```powershell
# Make sure LibraryApi migration was applied first
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef database update

# Then run NotificationService
cd ..\NotificationService
dotnet run
```

---

Need help with the migration? Let me know what error you're seeing!
