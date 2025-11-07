# ========================================
# Database Migration Script
# Creates Users and Notifications tables
# ========================================

Write-Host "=== Library API Database Migration ===" -ForegroundColor Green
Write-Host ""

# Step 1: Check if PostgreSQL is running
Write-Host "Step 1: Checking PostgreSQL connection..." -ForegroundColor Yellow
try {
    $pgTest = psql -U postgres -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Cannot connect to PostgreSQL. Make sure it's running." -ForegroundColor Red
        Write-Host "Start PostgreSQL with: docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15" -ForegroundColor Cyan
        exit 1
    }
    Write-Host "? PostgreSQL is running" -ForegroundColor Green
} catch {
    Write-Host "ERROR: PostgreSQL not found. Install PostgreSQL or run via Docker." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Backup existing data (optional)
Write-Host "Step 2: Do you want to backup existing data? (Y/N)" -ForegroundColor Yellow
$backup = Read-Host
if ($backup -eq "Y" -or $backup -eq "y") {
    $backupPath = "C:\temp\librarydb_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    Write-Host "Creating backup at: $backupPath" -ForegroundColor Cyan
    pg_dump -U postgres LibraryDb > $backupPath
    Write-Host "? Backup created" -ForegroundColor Green
}

Write-Host ""

# Step 3: Drop and recreate database
Write-Host "Step 3: Recreating database (this will delete all data)..." -ForegroundColor Yellow
Write-Host "Are you sure? (Y/N)" -ForegroundColor Red
$confirm = Read-Host
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Migration cancelled." -ForegroundColor Yellow
    exit 0
}

psql -U postgres -c 'DROP DATABASE IF EXISTS "LibraryDb";' 2>&1 | Out-Null
psql -U postgres -c 'CREATE DATABASE "LibraryDb";' 2>&1 | Out-Null
Write-Host "? Database recreated" -ForegroundColor Green

Write-Host ""

# Step 4: Remove old migrations
Write-Host "Step 4: Removing old migrations from LibraryApi..." -ForegroundColor Yellow
$migrationsPath = "C:\Personals\LibraryApi\LibraryApi\Data\Migrations"
if (Test-Path $migrationsPath) {
    Remove-Item -Path "$migrationsPath\*" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "? Old migrations removed" -ForegroundColor Green
} else {
    Write-Host "! No existing migrations found" -ForegroundColor Yellow
}

Write-Host ""

# Step 5: Create new migration
Write-Host "Step 5: Creating new migration..." -ForegroundColor Yellow
Set-Location "C:\Personals\LibraryApi\LibraryApi"

# Check if dotnet-ef is installed
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "Installing dotnet-ef tool..." -ForegroundColor Cyan
    dotnet tool install --global dotnet-ef --version 8.0.0
}

dotnet ef migrations add InitialCreateWithUsers
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create migration" -ForegroundColor Red
    Write-Host "Try installing EF tools manually: dotnet tool install --global dotnet-ef" -ForegroundColor Cyan
    exit 1
}
Write-Host "? Migration created" -ForegroundColor Green

Write-Host ""

# Step 6: Apply migration
Write-Host "Step 6: Applying migration to database..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to apply migration" -ForegroundColor Red
    exit 1
}
Write-Host "? Migration applied successfully" -ForegroundColor Green

Write-Host ""

# Step 7: Verify tables
Write-Host "Step 7: Verifying database tables..." -ForegroundColor Yellow
$tables = psql -U postgres -d LibraryDb -c "\dt" 2>&1
Write-Host $tables
Write-Host ""

# Check if Users table exists
$usersTable = psql -U postgres -d LibraryDb -c "SELECT COUNT(*) FROM \"Users\";" 2>&1
if ($usersTable -match "\d+") {
    Write-Host "? Users table exists" -ForegroundColor Green
} else {
    Write-Host "! Users table not found" -ForegroundColor Red
}

# Check if Notifications table exists
$notificationsTable = psql -U postgres -d LibraryDb -c "SELECT COUNT(*) FROM \"Notifications\";" 2>&1
if ($notificationsTable -match "\d+") {
    Write-Host "? Notifications table exists" -ForegroundColor Green
} else {
    Write-Host "! Notifications table not found" -ForegroundColor Red
}

Write-Host ""

# Step 8: Display seeded data
Write-Host "Step 8: Displaying seeded data..." -ForegroundColor Yellow
Write-Host "Users:" -ForegroundColor Cyan
psql -U postgres -d LibraryDb -c 'SELECT "UserId", "UserName", "Email", "MembershipType" FROM "Users";'
Write-Host ""
Write-Host "Books:" -ForegroundColor Cyan
psql -U postgres -d LibraryDb -c 'SELECT "Id", "Title", "Author", "IsAvailable" FROM "Books";'
Write-Host ""

# Step 9: Test LibraryApi
Write-Host "Step 9: Would you like to start LibraryApi? (Y/N)" -ForegroundColor Yellow
$startApi = Read-Host
if ($startApi -eq "Y" -or $startApi -eq "y") {
    Write-Host "Starting LibraryApi..." -ForegroundColor Cyan
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
    Start-Sleep -Seconds 2
    dotnet run --project "C:\Personals\LibraryApi\LibraryApi\LibraryApi.csproj"
}

Write-Host ""
Write-Host "=== Migration Complete ===" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Test LibraryApi: cd C:\Personals\LibraryApi\LibraryApi && dotnet run" -ForegroundColor White
Write-Host "2. Test NotificationService: cd C:\Personals\LibraryApi\NotificationService && dotnet run" -ForegroundColor White
Write-Host "3. Open Swagger: http://localhost:5262/swagger (LibraryApi)" -ForegroundColor White
Write-Host "4. Open Swagger: http://localhost:5089/swagger (NotificationService)" -ForegroundColor White
