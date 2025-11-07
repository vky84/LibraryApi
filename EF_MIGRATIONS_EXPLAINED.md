# EF Migrations - Understanding Your Setup

## ?? TL;DR (Too Long; Didn't Read)

You have **2 projects** sharing **1 database**. Here's how to handle migrations:

**? DO THIS:**
1. Manage ALL migrations in **LibraryApi only**
2. NotificationService just connects and uses existing tables
3. Run `dotnet ef migrations add` from LibraryApi project
4. Both projects call `MigrateAsync()` on startup (it's safe - won't duplicate)

**? DON'T DO THIS:**
- Don't create separate migrations in NotificationService
- Don't try to apply migrations from both projects
- Don't manually create tables outside of migrations

---

## ?? Background: Why This is Confusing

### The Shared Database Pattern

```
???????????????????????????????????????????????????
?         PostgreSQL (LibraryDb)                  ?
???????????????????????????????????????????????????
?  Books            ? Created by LibraryApi       ?
?  BorrowingRecords ? Created by LibraryApi       ?
?  Users            ? Created by LibraryApi       ?
?  Notifications    ? Created by LibraryApi       ?
?                                                  ?
?  __EFMigrationsHistory ? Tracks what's applied  ?
???????????????????????????????????????????????????
         ?                           ?
         ?                           ?
    LibraryApi                 NotificationService
    (Writes to all)            (Reads Books/BorrowingRecords,
                                Writes to Users/Notifications)
```

### Why Two DbContexts?

You have **two separate `LibraryDbContext.cs` files**:

1. **LibraryApi/Data/LibraryDbContext.cs**
   - Originally had: Books, BorrowingRecords
   - Should be updated to: Books, BorrowingRecords, Users, Notifications

2. **NotificationService/Data/LibraryDbContext.cs**
   - Has: Books (read-only), BorrowingRecords (read-only), Users, Notifications
   - Same database, different namespace

**The Problem:** EF Core sees these as different contexts, but they point to the same DB!

---

## ?? How EF Migrations Work

### Migration Tracking

EF Core uses a table called `__EFMigrationsHistory` to track what's been applied:

```sql
SELECT * FROM "__EFMigrationsHistory";

 MigrationId                      | ProductVersion
----------------------------------+----------------
 20251030161505_InitialCreate     | 8.0.0
 20250108120000_AddUsersAndNotifs | 8.0.0  ? New migration
```

### When You Run `MigrateAsync()`

```csharp
await context.Database.MigrateAsync();
```

EF Core:
1. Connects to database
2. Checks `__EFMigrationsHistory` table
3. Finds migrations in code that aren't in the table
4. Applies missing migrations
5. Updates `__EFMigrationsHistory`

**Key Point:** It's **idempotent** - safe to call multiple times!

---

## ? Correct Setup for Your Project

### Step 1: Update LibraryApi DbContext

**File:** `LibraryApi/Data/LibraryDbContext.cs`

Make sure it has ALL tables:

```csharp
public class LibraryDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<BorrowingRecord> BorrowingRecords { get; set; }
    public DbSet<User> Users { get; set; }  // ? ADD THIS
    public DbSet<Notification> Notifications { get; set; }  // ? ADD THIS
    
    // ... OnModelCreating with all table configurations
}
```

### Step 2: Add Model Classes to LibraryApi

Copy from NotificationService:

```bash
# PowerShell - copy User and Notification models
Copy-Item "..\NotificationService\Models\User.cs" "Models\User.cs"
Copy-Item "..\NotificationService\Models\Notification.cs" "Models\Notification.cs"
```

Or create manually in `LibraryApi/Models/`:

```csharp
// User.cs
public class User
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // ... other properties
}

// Notification.cs
public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    // ... other properties
}
```

### Step 3: Create Migration (LibraryApi ONLY)

```bash
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add AddUsersAndNotifications
```

This creates migration files in `LibraryApi/Data/Migrations/`.

### Step 4: Apply Migration

```bash
dotnet ef database update
```

Or just run the app (auto-migration on startup):

```bash
dotnet run
```

### Step 5: NotificationService Setup

**Keep `MigrateAsync()` in NotificationService** - it's safe!

```csharp
// NotificationService/Program.cs
// This is CORRECT - don't remove it
await context.Database.MigrateAsync();
```

**Why it's safe:**
- NotificationService sees the migrations history
- Recognizes migrations are already applied
- Does nothing (no-op)
- If new migrations are added to LibraryApi, it will apply them

---

## ?? Understanding the Error

### "Migrations already exist"

**Full error:**
```
System.InvalidOperationException: There is already an object named 'Books' in the database.
```

**Cause:**
- Database already has tables from LibraryApi
- Trying to create migration that would create same tables again

**Solution:**
```bash
# Don't create migrations in NotificationService!
# Only create them in LibraryApi
```

### "Cannot find DbContext"

**Full error:**
```
Unable to create an object of type 'LibraryDbContext'
```

**Cause:**
- Running migration command from wrong directory
- Missing connection string

**Solution:**
```bash
# Always run from project directory containing .csproj
cd C:\Personals\LibraryApi\LibraryApi

# Make sure appsettings.json has connection string
dotnet ef migrations add MigrationName
```

---

## ?? Command Cheat Sheet

### Creating Migrations

```bash
# From LibraryApi project directory
cd C:\Personals\LibraryApi\LibraryApi

# Create new migration
dotnet ef migrations add DescriptiveName

# Examples:
dotnet ef migrations add AddUsersAndNotifications
dotnet ef migrations add AddEmailToUser
dotnet ef migrations add CreateIndexOnNotifications
```

### Applying Migrations

```bash
# Apply all pending migrations
dotnet ef database update

# Apply to specific migration
dotnet ef database update MigrationName

# Rollback all migrations (dangerous!)
dotnet ef database update 0
```

### Viewing Migrations

```bash
# List all migrations
dotnet ef migrations list

# Output:
# 20251030161505_InitialCreate (Applied)
# 20250108120000_AddUsersAndNotifications (Pending)
```

### Removing Migrations

```bash
# Remove last migration (only if not applied!)
dotnet ef migrations remove

# If already applied, rollback first:
dotnet ef database update PreviousMigrationName
dotnet ef migrations remove
```

### Generating SQL Scripts

```bash
# Generate SQL for all migrations
dotnet ef migrations script --output schema.sql

# Generate SQL for specific range
dotnet ef migrations script FromMigration ToMigration --output update.sql

# For production deployment (review before applying!)
dotnet ef migrations script --idempotent --output production.sql
```

---

## ?? Development Workflow

### Daily Development

```bash
# 1. Make changes to models or DbContext in LibraryApi

# 2. Create migration
cd C:\Personals\LibraryApi\LibraryApi
dotnet ef migrations add DescribeYourChange

# 3. Review generated migration file
code Data/Migrations/20250108XXXXXX_DescribeYourChange.cs

# 4. Test locally
dotnet ef database update

# 5. Run LibraryApi to verify
dotnet run

# 6. Run NotificationService to verify
cd ..\NotificationService
dotnet run

# 7. Commit migration files
git add Data/Migrations/
git commit -m "Add new migration: DescribeYourChange"
```

### Adding New Table

```bash
# Example: Adding "Reviews" table

# 1. Create model: LibraryApi/Models/Review.cs
public class Review
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string Comment { get; set; }
    // ...
}

# 2. Add DbSet to LibraryDbContext
public DbSet<Review> Reviews { get; set; }

# 3. Configure in OnModelCreating
modelBuilder.Entity<Review>(entity => {
    entity.HasKey(e => e.Id);
    // ...
});

# 4. Create migration
dotnet ef migrations add AddReviewsTable

# 5. Apply migration
dotnet ef database update
```

### Changing Existing Column

```bash
# Example: Making Email required

# 1. Update model
public string Email { get; set; } = string.Empty;  // Already is string
// In OnModelCreating:
entity.Property(e => e.Email).IsRequired();  // Add this

# 2. Create migration
dotnet ef migrations add MakeEmailRequired

# 3. Review migration - EF Core generates:
migrationBuilder.AlterColumn<string>(
    name: "Email",
    table: "Users",
    nullable: false,  // Changed from nullable: true
    oldNullable: true);

# 4. Apply migration
dotnet ef database update
```

---

## ?? Troubleshooting Scenarios

### Scenario 1: Fresh Development Machine

**Situation:** New team member clones repo, database doesn't exist.

**Steps:**
```bash
# 1. Create database
psql -U postgres
CREATE DATABASE "LibraryDb";
\q

# 2. Apply all migrations
cd LibraryApi
dotnet ef database update

# 3. Run app
dotnet run
```

### Scenario 2: Production Deployment

**Situation:** Deploying new version with migrations to production.

**Steps:**
```bash
# 1. Generate idempotent SQL script
dotnet ef migrations script --idempotent --output deploy.sql

# 2. Review script carefully!
code deploy.sql

# 3. Backup production database
pg_dump -U postgres LibraryDb > backup_$(date +%Y%m%d).sql

# 4. Apply script in maintenance window
psql -U postgres -d LibraryDb -f deploy.sql

# 5. Deploy new application version
kubectl set image deployment/libraryapi ...
```

### Scenario 3: Conflicting Migrations

**Situation:** Two developers create migrations from same base, causing conflict.

**Steps:**
```bash
# Developer A created: 20250108120000_AddUserAddress
# Developer B created: 20250108120030_AddUserPhone
# Both are in main branch now

# Solution: Rebase and recreate migration
git pull origin main

# Remove your migration
dotnet ef migrations remove

# Create fresh migration
dotnet ef migrations add AddUserPhone

# Now timestamps won't conflict
```

---

## ? Best Practices Summary

1. **Single Migration Source**
   - ? LibraryApi owns all migrations
   - ? NotificationService doesn't create its own migrations

2. **Always From Project Directory**
   ```bash
   cd C:\Personals\LibraryApi\LibraryApi  # ? Correct
   dotnet ef migrations add MyMigration
   ```

3. **Descriptive Migration Names**
   ```bash
   # ? Good
   dotnet ef migrations add AddEmailIndexToUsers
   dotnet ef migrations add CreateNotificationsTable
   
   # ? Bad
   dotnet ef migrations add Update1
   dotnet ef migrations add FixStuff
   ```

4. **Review Before Applying**
   ```bash
   # Always review the generated migration
   code Data/Migrations/20250108XXXXXX_MigrationName.cs
   ```

5. **Keep Migrations in Source Control**
   ```bash
   git add Data/Migrations/
   git commit -m "Add migration: descriptive name"
   ```

6. **Production Safety**
   ```bash
   # Generate SQL script for review
   dotnet ef migrations script --idempotent > production.sql
   
   # Don't use auto-migration in production
   # Remove or comment out in Program.cs for production:
   // if (!app.Environment.IsProduction()) {
   //     await context.Database.MigrateAsync();
   // }
   ```

---

## ?? Additional Resources

**Entity Framework Core Documentation:**
- [Migrations Overview](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [Managing Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/managing)
- [Applying Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/applying)

**Your Project Files:**
- [EF_MIGRATIONS_GUIDE.md](./EF_MIGRATIONS_GUIDE.md) - Comprehensive guide
- [EF_QUICK_FIX.md](./EF_QUICK_FIX.md) - Quick solutions for common errors

---

**Still stuck?** Share your exact error message and I'll help debug it!
