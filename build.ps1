# Build Script for Microservice Manager
# This script builds the application in Release mode

Write-Host "Building Microservice Manager..." -ForegroundColor Cyan

# Restore NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nFailed to restore packages!" -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    Write-Host "`nTo run the application, use:" -ForegroundColor Cyan
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host "`nOr create a standalone executable with:" -ForegroundColor Cyan
    Write-Host "  .\publish.ps1" -ForegroundColor White
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

