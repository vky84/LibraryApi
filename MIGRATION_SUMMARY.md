# ? Solution: Database Migration with Users Table

## ?? What Was the Problem?

1. **No Users table** - User information was duplicated in BorrowingRecords (UserId, UserName)
2. **NotificationService expected Users table** - But it didn't exist in the database
3. **No migrations for Users/Notifications** - These tables were never created
4. **Unclear migration ownership** - Both projects trying to manage the same database

---

## ? What I've Fixed

### 1. Created User Model in LibraryApi ?

**File:** `LibraryApi/Models/User.cs`

```csharp
public class User
{
    public int Id { get; set; }
    public string UserId { get; set; }      // "user1", "user2", etc.
    public string UserName { get; set; }    // "John Doe"
    public string Email { get; set; }       // "john.doe@example.com"
    public string FullName { get; set; }
    public string MembershipType { get; set; }  // "Standard" or "Premium"
    public DateTime JoinedDate { get; set; }
    public bool IsActive { get; set; }
}
```

### 2. Created Notification Model in LibraryApi ?

**File:** `LibraryApi/Models/Notification.cs`

Full notification tracking with:
- UserId, UserEmail, UserName
- Type (enum): BookBorrowed, DueSoonReminder, OverdueNotice, etc.
- Subject, Message
- CreatedAt, ScheduledFor, IsSent, SentAt
- RetryCount, ErrorMessage

### 3. Updated LibraryApi DbContext ?

**File:** `LibraryApi/Data/LibraryDbContext.cs`

Added:
- `DbSet<User> Users`
- `DbSet<Notification> Notifications`
- Proper table configurations
- Seeded 3 users with emails

### 4. Created Migration Plan ?

**File:** `DATABASE_MIGRATION_PLAN.md`

Comprehensive guide explaining:
- Why we need a Users table
- New database schema
- Migration strategies
- Where to place migrations

### 5. Created Automated Migration Script ?

**File:** `migrate-database.ps1`

PowerShell script that:
- Checks PostgreSQL connection
- Backs up existing data (optional)
- Drops and recreates database
- Removes old migrations
- Creates new migration
- Applies migration
- Verifies tables exist
- Displays seeded data

---

## ?? New Database Structure

```
LibraryDb
??? Books (existing)
?   ??? Id, Title, Author, ISBN, IsAvailable, etc.
?
??? Users (NEW) ?
?   ??? Id (Primary Key)
?   ??? UserId (Unique: "user1", "user2", "user3")
?   ??? UserName ("John Doe")
?   ??? Email (Unique: "john.doe@example.com")
?   ??? FullName
?   ??? MembershipType
?   ??? JoinedDate
?   ??? IsActive
?
??? BorrowingRecords (existing, now references Users)
?   ??? Id
?   ??? BookId (FK ? Books.Id)
?   ??? UserId (References Users.UserId)
?   ??? UserName (Denormalized for convenience)
?   ??? BorrowedDate
?   ??? DueDate
?   ??? ReturnedDate
?
??? Notifications (NEW) ?
    ??? Id
    ??? UserId (References Users.UserId)
    ??? UserEmail
    ??? Type (enum)
    ??? Subject
    ??? Message
    ??? BookId (optional)
    ??? BorrowingRecordId (optional)
    ??? CreatedAt
    ??? ScheduledFor
    ??? IsSent
    ??? SentAt
    ??? RetryCount
    ??? ErrorMessage
```

---

## ?? How to Apply the Changes

### Option 1: Automated Script (Recommended)

```powershell
# Run the migration script
cd C:\Personals\LibraryApi
.\migrate-database.ps1
```

This will:
1. ? Check PostgreSQL is running
2. ? Backup existing data (if you choose)
3. ? Recreate the database
4. ? Remove old migrations
5. ? Create new comprehensive migration
6. ? Apply migration to database
7. ? Verify all tables exist
8. ? Show seeded data

### Option 2: Manual Steps

```powershell
# 1. Drop and recreate database
psql -U postgres
DROP DATABASE IF EXISTS "LibraryDb";
CREATE DATABASE "LibraryDb";
\q

# 2. Navigate to LibraryApi
cd C:\Personals\LibraryApi\LibraryApi

# 3. Remove old migrations
Remove-Item -Path "Data\Migrations\*" -Recurse -Force

# 4. Install EF tools (if not already installed)
dotnet tool install --global dotnet-ef --version 8.0.0

# 5. Create migration
dotnet ef migrations add InitialCreateWithUsers

# 6. Apply migration
dotnet ef database update

# 7. Run LibraryApi
dotnet run
```

---

## ?? Migration Ownership Answer

### ? Where Should Migrations Be Placed?

**Answer: LibraryApi should own ALL migrations** ?

```
C:\Personals\LibraryApi\
??? LibraryApi\
?   ??? Data\
?       ??? Migrations\          ? ALL migrations here
?           ??? YYYYMMDD_InitialCreateWithUsers.cs
?
??? NotificationService\
    ??? Data\
        ??? (NO Migrations folder)
```

**Why?**
1. ? **Single source of truth** - All schema changes in one place
2. ? **No conflicts** - Only one project creates migrations
3. ? **Clear ownership** - LibraryApi is the "database owner"
4. ? **Both services can call MigrateAsync()** - It's idempotent (safe)
5. ? **Easier to maintain** - Update schema in one location

**NotificationService:**
- ? Uses the same database
- ? Reads from Users, Notifications tables
- ? Can call `MigrateAsync()` safely (won't duplicate)
- ? Does NOT create its own migrations
- ? Does NOT have a Migrations folder

---

## ?? Development Workflow Going Forward

### When You Need to Change the Database:

```powershell
# 1. Edit models in LibraryApi/Models/
# Example: Add PhoneNumber to User.cs

# 2. Update DbContext configuration (if needed)
# LibraryApi/Data/LibraryDbContext.cs

# 3. Create migration in LibraryApi
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add AddPhoneNumberToUser

# 4. Review migration file
code Data/Migrations/YYYYMMDD_AddPhoneNumberToUser.cs

# 5. Apply migration
dotnet ef database update

# 6. Test LibraryApi
dotnet run

# 7. Test NotificationService
cd ..\NotificationService
dotnet run
# Will automatically apply the same migration (safely)
```

---

## ?? Seeded Data

After running the migration, you'll have:

### Users
| UserId | UserName    | Email                   | MembershipType |
|--------|-------------|-------------------------|----------------|
| user1  | John Doe    | john.doe@example.com    | Standard       |
| user2  | Jane Smith  | jane.smith@example.com  | Premium        |
| user3  | Bob Johnson | bob.johnson@example.com | Standard       |

### Books
- The Great Gatsby
- To Kill a Mockingbird
- 1984
- Pride and Prejudice
- The Catcher in the Rye

### BorrowingRecords
- user1 borrowed "1984" (overdue)
- user2 borrowed "The Great Gatsby" (returned)

---

## ? Verification Checklist

After migration, verify:

```powershell
# 1. Check tables exist
psql -U postgres -d LibraryDb -c "\dt"
# Should show: Books, Users, BorrowingRecords, Notifications

# 2. Check Users data
psql -U postgres -d LibraryDb -c 'SELECT * FROM "Users";'
# Should show 3 users with emails

# 3. Run LibraryApi
cd C:\Personals\LibraryApi\LibraryApi
dotnet run
# Check console: "DATABASE INITIALIZED SUCCESSFULLY"

# 4. Run NotificationService
cd ..\NotificationService
dotnet run
# Check console: "Users in database: 3"  ? This should work now!

# 5. Test notification endpoint
# Use NotificationService.http file
POST http://localhost:5089/api/notifications/send
{
  "userId": "user1",
  "subject": "Test",
  "message": "Hello!"
}
# Should succeed and find user's email
```

---

## ?? Architecture Benefits

### For Your Academic Project

1. **Proper Database Design**
   - ? Normalized data (Users in separate table)
   - ? No data duplication
   - ? Referential integrity

2. **Microservices Pattern**
   - ? One service owns the schema (LibraryApi)
   - ? Other services use it (NotificationService)
   - ? Clear responsibilities

3. **Maintainability**
   - ? Single migration source
   - ? Easy to update schema
   - ? No conflicts

4. **Scalability**
   - ? Can add more microservices easily
   - ? All use the same database schema
   - ? Consistent data model

---

## ?? Documentation Files Created

1. **DATABASE_MIGRATION_PLAN.md** - Comprehensive guide
2. **migrate-database.ps1** - Automated migration script
3. **THIS FILE** - Quick summary

---

## ?? Troubleshooting

### "dotnet-ef not found"
```powershell
dotnet tool install --global dotnet-ef --version 8.0.0
# Or
dotnet tool update --global dotnet-ef
```

### "Cannot connect to PostgreSQL"
```powershell
# Start PostgreSQL with Docker
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Or check if it's running
docker ps | grep postgres
```

### "Users table doesn't exist" in NotificationService
```powershell
# Make sure LibraryApi migration was applied
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef database update

# Check tables
psql -U postgres -d LibraryDb -c "\dt"
```

### "Migration already exists"
```powershell
# Remove old migrations
cd C:\Personals\LibraryApi\LibraryApi
Remove-Item -Path "Data\Migrations\*" -Recurse -Force

# Create fresh migration
dotnet ef migrations add InitialCreateWithUsers
```

---

## ? Next Steps

1. ? **Run the migration script:** `.\migrate-database.ps1`
2. ? **Verify tables exist:** Check Users, Notifications tables
3. ? **Test LibraryApi:** `dotnet run` in LibraryApi folder
4. ? **Test NotificationService:** Should now see "Users in database: 3"
5. ? **Send test notification:** Use NotificationService.http file
6. ? **Update documentation:** Reflect Users table in architecture docs

---

**Status: Ready to migrate!** Run `.\migrate-database.ps1` to apply all changes.
