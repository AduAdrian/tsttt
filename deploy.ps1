# Miseda Inspect SRL - Deployment Script
# Run this in PowerShell to publish the application for production

param(
    [string]$OutputPath = ".\bin\Release\net8.0\publish",
    [string]$Configuration = "Release"
)

Write-Host "?? Starting deployment for Miseda Inspect SRL..." -ForegroundColor Green
Write-Host "Target Domain: www.misedainspectsrl.ro" -ForegroundColor Yellow

# Navigate to project directory
Set-Location "WebApplication1"

# Clean previous builds
Write-Host "?? Cleaning previous builds..." -ForegroundColor Blue
dotnet clean --configuration $Configuration

# Restore packages
Write-Host "?? Restoring NuGet packages..." -ForegroundColor Blue
dotnet restore

# Build the application
Write-Host "?? Building application..." -ForegroundColor Blue
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed!" -ForegroundColor Red
    exit 1
}

# Publish the application
Write-Host "?? Publishing application..." -ForegroundColor Blue
dotnet publish --configuration $Configuration --output $OutputPath --no-build --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "? Application published successfully!" -ForegroundColor Green
Write-Host "?? Output path: $OutputPath" -ForegroundColor Yellow

# Display deployment notes
Write-Host ""
Write-Host "?? DEPLOYMENT NOTES:" -ForegroundColor Cyan
Write-Host "1. Upload the contents of '$OutputPath' to your web server" -ForegroundColor White
Write-Host "2. Ensure your domain www.misedainspectsrl.ro points to the server" -ForegroundColor White
Write-Host "3. Configure SSL certificate for HTTPS" -ForegroundColor White
Write-Host "4. Ensure SQL Server/SQLite database is accessible" -ForegroundColor White
Write-Host "5. Test email settings with mail.misedainspectsrl.ro" -ForegroundColor White
Write-Host "6. Verify SMS settings are working" -ForegroundColor White
Write-Host ""
Write-Host "?? FIRST TIME SETUP:" -ForegroundColor Cyan
Write-Host "- Access: https://www.misedainspectsrl.ro/Account/Login" -ForegroundColor White
Write-Host "- Default admin: admin / Admin123!" -ForegroundColor White
Write-Host "- Default email: notificari-sms@misedainspectsrl.ro" -ForegroundColor White
Write-Host ""
Write-Host "?? Ready for production deployment!" -ForegroundColor Green