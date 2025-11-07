# Entity Framework Migrations Guide - LibraryApi & NotificationService

## ?? Understanding the Problem

You have **TWO separate projects** sharing the **SAME database**, but each with their own `DbContext` and migrations:

```
C:\Personals\LibraryApi\
??? LibraryApi\
?   ??? Data\LibraryDbContext.cs        ? Manages Books, BorrowingRecords
?   ??? Data\Migrations\
?       ??? 20251030161505_InitialCreate.cs
?
??? NotificationService\
    ??? Data\LibraryDbContext.cs        ? Manages Users, Notifications (+ reads Books, BorrowingRecords)
    ??? Data\Migrations\
        ??? (NO MIGRATIONS YET!)
```

### The Issue

When you run **NotificationService**, it tries to apply migrations but:
- ? The database already has tables from **LibraryApi** migrations
- ? **NotificationService** has its own `DbContext` but **NO migrations created yet**
- ? Both contexts try to manage the **SAME database** (`LibraryDb`)

---

## ? Solution Options

You have **3 approaches** to solve this:

### **Option 1: Single Migration Set (Recommended for Shared Database)** ?

Maintain migrations in **ONE project only** (LibraryApi) and have NotificationService just apply them.

### **Option 2: Separate Migrations per Service** 

Each service maintains its own migrations but coordinates carefully.

### **Option 3: Manual SQL Scripts**

Skip EF migrations entirely and use raw SQL.

---

## ?? Recommended Solution: Option 1 (Single Migration Set)

Since both services share the same database, manage ALL migrations from **LibraryApi** project.

### Step 1: Update LibraryApi DbContext to Include All Tables

**File:** `LibraryApi/Data/LibraryDbContext.cs`

Add the Users and Notifications tables:

```csharp
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;

namespace LibraryApi.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        // Existing tables
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
        
        // NEW: Tables for NotificationService
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing configuration...
            
            // NEW: Add User and Notification configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.MembershipType).HasMaxLength(50);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                
                entity.HasIndex(e => new { e.UserId, e.IsSent });
                entity.HasIndex(e => new { e.IsSent, e.ScheduledFor });
            });
            
            // Add seed data for Users...
        }
    }
}
```

### Step 2: Add Model Classes to LibraryApi

Copy `User.cs` and `Notification.cs` from NotificationService to LibraryApi:

```bash
# PowerShell
Copy-Item "C:\Personals\LibraryApi\NotificationService\Models\User.cs" `
          "C:\Personals\LibraryApi\LibraryApi\Models\User.cs"

Copy-Item "C:\Personals\LibraryApi\NotificationService\Models\Notification.cs" `
          "C:\Personals\LibraryApi\LibraryApi\Models\Notification.cs"
```

### Step 3: Create New Migration in LibraryApi

```bash
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add AddUsersAndNotifications
```

This creates:
```
LibraryApi/Data/Migrations/
??? 20251030161505_InitialCreate.cs
??? 20251030161505_InitialCreate.Designer.cs
??? 20250108XXXXXX_AddUsersAndNotifications.cs    ? NEW
??? 20250108XXXXXX_AddUsersAndNotifications.Designer.cs
??? LibraryDbContextModelSnapshot.cs
```

### Step 4: Apply Migration

```bash
# Option A: Manual (recommended for first time)
dotnet ef database update

# Option B: Let it auto-apply on startup (already configured)
dotnet run
```

### Step 5: Configure NotificationService to Use Same Migrations

**NotificationService doesn't need its own migrations!**

It will just connect to the existing database and use the tables.

**Make sure NotificationService's DbContext matches LibraryApi's:**

```csharp
// NotificationService/Data/LibraryDbContext.cs
// Keep the SAME table configurations as LibraryApi
```

### Step 6: Remove Auto-Migration from NotificationService (Optional)

If you don't want NotificationService to try applying migrations:

**File:** `NotificationService/Program.cs`

```csharp
// OPTION 1: Keep auto-migration (safe - won't re-create existing tables)
await context.Database.MigrateAsync();  // ? Safe - EF tracks what's applied

// OPTION 2: Just ensure database exists (no migration apply)
// await context.Database.EnsureCreatedAsync();  // ?? DON'T USE with migrations

// OPTION 3: Skip migration entirely (rely on LibraryApi to do it)
// Comment out the migration code
```

**Recommended:** Keep `MigrateAsync()` - it's safe and idempotent!

---

## ?? Complete Step-by-Step Instructions

### Clean Slate Approach (If Database Already Has Tables)

```bash
# 1. Drop and recreate database (WARNING: Deletes all data!)
psql -U postgres -c "DROP DATABASE IF EXISTS \"LibraryDb\";"
psql -U postgres -c "CREATE DATABASE \"LibraryDb\";"

# 2. Remove existing migrations from LibraryApi
cd C:\Personals\LibraryApi\LibraryApi
Remove-Item -Path "Data\Migrations\*" -Force

# 3. Create a fresh comprehensive migration
dotnet ef migrations add InitialCreate

# 4. Apply migration
dotnet ef database update

# 5. Run LibraryApi (should work)
dotnet run

# 6. Run NotificationService (should also work)
cd ..\NotificationService
dotnet run
```

### Incremental Approach (Keep Existing Data)

```bash
# 1. Check current database state
dotnet ef migrations list --project LibraryApi.csproj

# 2. Create migration for new tables
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add AddUsersAndNotifications

# 3. Review migration file to ensure it only adds Users and Notifications

# 4. Apply migration
dotnet ef database update

# 5. Verify tables exist
psql -U postgres -d LibraryDb -c "\dt"
# Should show: Books, BorrowingRecords, Users, Notifications

# 6. Run both services
dotnet run  # LibraryApi
cd ..\NotificationService
dotnet run  # NotificationService
```

---

## ?? Troubleshooting Common Errors

### Error: "Table 'Books' already exists"

**Cause:** Trying to apply InitialCreate migration when database already has tables.

**Solution:**
```bash
# Option A: Mark migration as already applied
dotnet ef database update 0  # Rollback all
dotnet ef database update    # Reapply

# Option B: Drop and recreate database (loses data)
psql -U postgres -c "DROP DATABASE \"LibraryDb\";"
psql -U postgres -c "CREATE DATABASE \"LibraryDb\";"
dotnet ef database update
```

### Error: "Duplicate object name 'PK_Books'"

**Cause:** Multiple DbContexts trying to manage the same tables.

**Solution:**
- Use **ONE** DbContext for migrations (LibraryApi)
- NotificationService just reads from existing tables

### Error: "The following constructors parameters did not have matching fields"

**Cause:** Model classes don't match between LibraryApi and NotificationService.

**Solution:**
```bash
# Make sure models are identical or properly shared
# Copy models from NotificationService to LibraryApi
```

### Error: "No migrations configuration type was found"

**Cause:** Running migrations from wrong directory.

**Solution:**
```bash
# Always run from project directory
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add MigrationName
```

---

## ?? Best Practices for Your Setup

### 1. **Migration Ownership**

? **LibraryApi owns ALL migrations**
- Creates tables for Books, BorrowingRecords, Users, Notifications
- Single source of truth for schema

? **NotificationService does NOT create migrations**
- Just connects to existing database
- Can still use `MigrateAsync()` (it's idempotent)

### 2. **Shared Models**

Consider creating a shared library:

```
C:\Personals\LibraryApi\
??? LibraryApi.Shared\        ? NEW
?   ??? Models\
?       ??? Book.cs
?       ??? BorrowingRecord.cs
?       ??? User.cs
?       ??? Notification.cs
??? LibraryApi\
?   ??? (references Shared)
??? NotificationService\
    ??? (references Shared)
```

### 3. **Development Workflow**

```bash
# 1. Make schema changes in LibraryApi/Data/LibraryDbContext.cs
# 2. Create migration
dotnet ef migrations add DescriptiveNameHere --project LibraryApi.csproj

# 3. Review migration file
code Data/Migrations/20250108XXXXXX_DescriptiveNameHere.cs

# 4. Apply to database
dotnet ef database update --project LibraryApi.csproj

# 5. Test LibraryApi
dotnet run --project LibraryApi.csproj

# 6. Test NotificationService
dotnet run --project NotificationService.csproj

# 7. Commit migration files to Git
git add Data/Migrations/
git commit -m "Add Users and Notifications tables"
```

### 4. **Production Deployment**

```bash
# DO NOT use auto-migration in production!

# Instead, generate SQL scripts:
dotnet ef migrations script --output migration.sql --project LibraryApi.csproj

# Review SQL
cat migration.sql

# Apply manually (Kubernetes Job or manual execution)
psql -U postgres -d LibraryDb -f migration.sql
```

---

## ?? Quick Reference Commands

### Create Migration
```bash
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add MigrationName
```

### Apply Migrations
```bash
dotnet ef database update
```

### List Migrations
```bash
dotnet ef migrations list
```

### Generate SQL Script
```bash
dotnet ef migrations script --output schema.sql
```

### Rollback Last Migration
```bash
dotnet ef migrations remove
```

### Rollback to Specific Migration
```bash
dotnet ef database update PreviousMigrationName
```

### Drop Database (WARNING: Deletes data!)
```bash
dotnet ef database drop --force
```

### Check Migration Status
```bash
dotnet ef migrations list
# Applied migrations shown with checkmark ?
```

---

## ? Recommended Action Plan for Your Current Situation

Based on your error, here's what I recommend:

```bash
# Step 1: Check what's in the database
psql -U postgres -d LibraryDb -c "\dt"

# Step 2: If you see Books and BorrowingRecords but NO Users or Notifications:
cd C:\Personals\LibraryApi\LibraryApi

# Step 3: Make sure User.cs and Notification.cs are in LibraryApi/Models/
# (Copy from NotificationService if needed)

# Step 4: Update LibraryApi/Data/LibraryDbContext.cs to include Users and Notifications

# Step 5: Create migration
dotnet ef migrations add AddUsersAndNotifications

# Step 6: Apply migration
dotnet ef database update

# Step 7: Run LibraryApi
dotnet run

# Step 8: Run NotificationService
cd ..\NotificationService
dotnet run
```

---

## ?? Emergency: Start Fresh

If everything is broken and you just want to start over:

```bash
# WARNING: This deletes all data!

# 1. Drop database
psql -U postgres -c "DROP DATABASE IF EXISTS \"LibraryDb\";"
psql -U postgres -c "CREATE DATABASE \"LibraryDb\";"

# 2. Clear all migrations from both projects
Remove-Item "C:\Personals\LibraryApi\LibraryApi\Data\Migrations\*" -Force
Remove-Item "C:\Personals\LibraryApi\NotificationService\Data\Migrations\*" -Force

# 3. Create comprehensive migration in LibraryApi only
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add InitialCreate

# 4. Apply migration
dotnet ef database update

# 5. Done! Both projects can now use the database
```

---

**Need specific help?** Share the exact error message you're getting, and I can provide targeted assistance!
