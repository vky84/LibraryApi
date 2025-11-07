# ?? Quick Start: Apply Database Migration

## ? Fastest Way to Get Running

### Step 1: Run the Automated Script

```powershell
# Open PowerShell in the solution directory
cd C:\Personals\LibraryApi

# Run the migration script
.\migrate-database.ps1
```

**The script will:**
1. Check PostgreSQL connection
2. Optionally backup your existing data
3. Drop and recreate the database
4. Create the migration
5. Apply the migration
6. Show you the seeded data
7. Optionally start LibraryApi

---

## ?? What Gets Created

### Tables
? **Users** - 3 seeded users with emails  
? **Books** - 5 sample books  
? **BorrowingRecords** - 2 sample borrowing records  
? **Notifications** - Empty table, ready for use  

### Seeded Users
```
user1 ? john.doe@example.com (Standard)
user2 ? jane.smith@example.com (Premium)
user3 ? bob.johnson@example.com (Standard)
```

---

## ? Verify It Worked

### Test 1: Check Database

```powershell
psql -U postgres -d LibraryDb -c 'SELECT "UserId", "Email" FROM "Users";'

# Expected output:
#  UserId |          Email
# --------+-------------------------
#  user1  | john.doe@example.com
#  user2  | jane.smith@example.com
#  user3  | bob.johnson@example.com
```

### Test 2: Run LibraryApi

```powershell
cd C:\Personals\LibraryApi\LibraryApi
dotnet run

# Look for:
# ? "DATABASE INITIALIZED SUCCESSFULLY"
# ? "Seeded 3 users" (might not show if already exists)
```

### Test 3: Run NotificationService

```powershell
cd C:\Personals\LibraryApi\NotificationService
dotnet run

# Look for:
# ? "Users in database: 3"  ? THIS IS THE KEY LINE!
# ? "DATABASE INITIALIZED SUCCESSFULLY"
```

### Test 4: Send a Notification

Open `NotificationService.http` and run:

```http
POST http://localhost:5089/api/notifications/send
Content-Type: application/json

{
  "userId": "user1",
  "subject": "Test Notification",
  "message": "This should work now!"
}
```

**Expected response:**
```json
{
  "message": "Notification sent successfully",
  "userId": "user1",
  "subject": "Test Notification"
}
```

**Check console for simulated email:**
```
=== SIMULATED EMAIL SEND ===
To: john.doe@example.com
Subject: Test Notification
Body: This should work now!
============================
```

---

## ?? If Script Fails

### Error: "dotnet-ef not found"

```powershell
# Install EF Core tools
dotnet tool install --global dotnet-ef --version 8.0.0

# Then run the script again
.\migrate-database.ps1
```

### Error: "Cannot connect to PostgreSQL"

```powershell
# Start PostgreSQL with Docker
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Wait 5 seconds, then run script
Start-Sleep -Seconds 5
.\migrate-database.ps1
```

### Error: "Permission denied"

```powershell
# Allow script execution
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Then run script
.\migrate-database.ps1
```

---

## ?? Manual Migration (If Script Doesn't Work)

```powershell
# 1. Drop database
psql -U postgres -c 'DROP DATABASE IF EXISTS "LibraryDb";'
psql -U postgres -c 'CREATE DATABASE "LibraryDb";'

# 2. Navigate to LibraryApi
cd C:\Personals\LibraryApi\LibraryApi

# 3. Remove old migrations
Remove-Item -Path "Data\Migrations\*" -Recurse -Force -ErrorAction SilentlyContinue

# 4. Create migration
dotnet ef migrations add InitialCreateWithUsers

# 5. Apply migration
dotnet ef database update

# 6. Verify
psql -U postgres -d LibraryDb -c 'SELECT * FROM "Users";'
```

---

## ? Success Criteria

You'll know it worked when:

1. ? `psql` shows Users table with 3 rows
2. ? LibraryApi starts without errors
3. ? NotificationService shows "Users in database: 3"
4. ? Sending notification to user1 succeeds
5. ? Console shows simulated email to john.doe@example.com

---

## ?? What This Fixes

### Before
? No Users table  
? NotificationService fails on startup  
? Cannot send notifications (no email addresses)  
? User data duplicated in BorrowingRecords  

### After
? Users table exists with 3 seeded users  
? NotificationService starts successfully  
? Can send notifications to real email addresses  
? Proper database normalization  
? Single source of truth for migrations (LibraryApi)  

---

## ?? More Information

- **Detailed guide:** See `DATABASE_MIGRATION_PLAN.md`
- **Architecture decisions:** See `MIGRATION_SUMMARY.md`
- **EF Migrations help:** See `EF_MIGRATIONS_GUIDE.md`

---

**Ready?** Run `.\migrate-database.ps1` now! ??
