# ========================================
# Fix HTTPS Development Certificate
# Run this as Administrator
# ========================================

Write-Host "=== HTTPS Certificate Fix for .NET Development ===" -ForegroundColor Green
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To run as Administrator:" -ForegroundColor Yellow
    Write-Host "1. Right-click PowerShell" -ForegroundColor Cyan
    Write-Host "2. Select 'Run as Administrator'" -ForegroundColor Cyan
    Write-Host "3. Run this script again" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "? Running as Administrator" -ForegroundColor Green
Write-Host ""

# Step 1: Check current certificate status
Write-Host "Step 1: Checking current certificate status..." -ForegroundColor Yellow
$certCheck = dotnet dev-certs https --check --trust 2>&1
Write-Host $certCheck
Write-Host ""

# Step 2: Clean existing certificates
Write-Host "Step 2: Cleaning existing certificates..." -ForegroundColor Yellow
$choice = Read-Host "Do you want to remove existing certificates? (Y/N)"
if ($choice -eq "Y" -or $choice -eq "y") {
    dotnet dev-certs https --clean
    Write-Host "? Existing certificates cleaned" -ForegroundColor Green
} else {
    Write-Host "Skipped certificate cleaning" -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Create and trust new certificate
Write-Host "Step 3: Creating and trusting new development certificate..." -ForegroundColor Yellow
Write-Host "You will see a Windows Security dialog - click YES to trust the certificate" -ForegroundColor Cyan
Start-Sleep -Seconds 2

$trustResult = dotnet dev-certs https --trust 2>&1
Write-Host $trustResult

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Certificate created and trusted successfully" -ForegroundColor Green
} else {
    Write-Host "? Failed to create/trust certificate" -ForegroundColor Red
    Write-Host "Error code: $LASTEXITCODE" -ForegroundColor Red
}
Write-Host ""

# Step 4: Verify certificate
Write-Host "Step 4: Verifying certificate..." -ForegroundColor Yellow
$verifyResult = dotnet dev-certs https --check --trust 2>&1
Write-Host $verifyResult
Write-Host ""

# Step 5: Test with NotificationService
Write-Host "Step 5: Testing with NotificationService..." -ForegroundColor Yellow
$testChoice = Read-Host "Do you want to test by running NotificationService? (Y/N)"

if ($testChoice -eq "Y" -or $testChoice -eq "y") {
    Write-Host ""
    Write-Host "Starting NotificationService..." -ForegroundColor Cyan
    Write-Host "Browser should open automatically to Swagger UI" -ForegroundColor Cyan
    Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Gray
    Write-Host ""
    Start-Sleep -Seconds 2
    
    Set-Location "C:\Personals\LibraryApi\NotificationService"
    dotnet run --launch-profile https
}

Write-Host ""
Write-Host "=== Certificate Fix Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Close all browser windows" -ForegroundColor White
Write-Host "2. Restart Visual Studio (if open)" -ForegroundColor White
Write-Host "3. Run NotificationService" -ForegroundColor White
Write-Host "4. Try: https://localhost:7230/swagger" -ForegroundColor White
Write-Host ""
Write-Host "If you still see certificate warnings:" -ForegroundColor Yellow
Write-Host "- Clear browser cache (Ctrl+Shift+Delete)" -ForegroundColor White
Write-Host "- Try incognito/private mode" -ForegroundColor White
Write-Host "- Restart your computer" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to exit"
