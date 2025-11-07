# Quick Fix: EF Migrations Conflict

## ?? The Problem

You have **two projects** (LibraryApi and NotificationService) both trying to manage migrations for the **same database** (LibraryDb).

**Current State:**
- ? LibraryApi has migrations for Books and BorrowingRecords  
- ? NotificationService needs Users and Notifications tables
- ? Both projects have separate DbContext files
- ? Conflict when both try to apply migrations

---

## ? Quick Fix (Choose One)

### Option A: Add Tables via LibraryApi Migration (Recommended) ?

This keeps ONE source of truth for all migrations.

**Run these commands:**

```powershell
# 1. Navigate to LibraryApi project
cd C:\Personals\LibraryApi\LibraryApi

# 2. Add migration for new tables
dotnet ef migrations add AddUsersAndNotifications

# 3. Apply migration
dotnet ef database update

# 4. Run LibraryApi
dotnet run

# In another terminal:
# 5. Run NotificationService
cd C:\Personals\LibraryApi\NotificationService
dotnet run
```

**Important:** Make sure `LibraryApi/Data/LibraryDbContext.cs` includes `DbSet<User>` and `DbSet<Notification>`.

---

### Option B: Skip NotificationService Migrations

Let NotificationService use the database without managing its own migrations.

**Edit:** `NotificationService/Program.cs`

**Change this:**
```csharp
await context.Database.MigrateAsync();
```

**To this:**
```csharp
// Skip migration - tables created by LibraryApi
// await context.Database.MigrateAsync();
logger.LogInformation("Skipping migrations - using database managed by LibraryApi");
```

Then run:
```powershell
# 1. Make sure LibraryApi has created all tables
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef database update

# 2. Run NotificationService
cd ..\NotificationService
dotnet run
```

---

### Option C: Fresh Start (Nuclear Option)

**WARNING: This deletes all data!**

```powershell
# 1. Drop and recreate database
psql -U postgres
DROP DATABASE IF EXISTS "LibraryDb";
CREATE DATABASE "LibraryDb";
\q

# 2. Apply LibraryApi migrations only
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef database update

# 3. Run both services
dotnet run

# In another terminal
cd ..\NotificationService
dotnet run
```

---

## ?? Check Current Database State

```powershell
# See what tables exist
psql -U postgres -d LibraryDb

# In psql prompt:
\dt

# Expected output:
#  Schema |       Name        | Type  |  Owner
# --------+-------------------+-------+----------
#  public | Books             | table | postgres
#  public | BorrowingRecords  | table | postgres
#  public | Notifications     | table | postgres
#  public | Users             | table | postgres
#  public | __EFMigrationsHistory | table | postgres
```

---

## ? What Error Are You Seeing?

### Error: "Table already exists"
```
Npgsql.PostgresException: 42P07: relation "Books" already exists
```

**Fix:** Database already has tables. Skip to fresh state or use Option B above.

---

### Error: "No such table: Users"
```
Npgsql.PostgresException: 42P01: relation "Users" does not exist
```

**Fix:** LibraryApi hasn't created Users table yet. Use Option A to add migration.

---

### Error: "The entity type 'User' requires a primary key"
```
InvalidOperationException: The entity type 'User' requires a primary key to be defined
```

**Fix:** DbContext configuration is missing. Check `OnModelCreating` in `LibraryDbContext.cs`.

---

## ?? Verify Configuration Files

### LibraryApi/Data/LibraryDbContext.cs

Should include:
```csharp
public DbSet<Book> Books { get; set; }
public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
public DbSet<User> Users { get; set; }  // ? Check this exists
public DbSet<Notification> Notifications { get; set; }  // ? Check this exists
```

### NotificationService/Data/LibraryDbContext.cs

Should be IDENTICAL to LibraryApi or reference the same shared context.

---

## ?? My Recommendation

**Use Option A** - it's the cleanest approach:

1. LibraryApi owns ALL database schema
2. NotificationService just uses the existing tables
3. Single source of truth for migrations

**Steps:**

```powershell
# 1. Ensure LibraryApi DbContext has all models
# Open: C:\Personals\LibraryApi\LibraryApi\Data\LibraryDbContext.cs
# Verify it has: Books, BorrowingRecords, Users, Notifications

# 2. Create migration
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add AddUsersAndNotifications

# 3. Apply migration
dotnet ef database update

# 4. Test LibraryApi
dotnet run
# Check console: "Database initialized successfully"

# 5. Test NotificationService
cd ..\NotificationService
dotnet run
# Check console: "Database initialized successfully"
```

---

## ?? Still Getting Errors?

**Share the exact error message and I can provide specific help!**

Common error messages:
- "Migrations already exist"
- "Table already exists"
- "No such table"
- "Unable to resolve service"
- "The entity type requires a primary key"

**To get detailed error info:**

```powershell
# Run with verbose logging
cd C:\Personals\LibraryApi\NotificationService
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --verbosity detailed
```

---

## ?? Quick Diagnostic

Run this to see what's happening:

```powershell
# Check database tables
psql -U postgres -d LibraryDb -c "SELECT tablename FROM pg_tables WHERE schemaname='public';"

# Check LibraryApi migrations
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations list

# Check NotificationService has no migrations (expected)
cd ..\NotificationService
dotnet ef migrations list
# Should show: "No migrations found" or use LibraryApi's migrations
```

---

**Need help?** Paste your error message and we'll debug it together!
