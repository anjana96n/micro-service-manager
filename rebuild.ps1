# Rebuild Script - Clean and Build
# Use this if you're having issues with the application

Write-Host "Cleaning and rebuilding Microservice Manager..." -ForegroundColor Cyan

# Clean
Write-Host "`nCleaning previous build..." -ForegroundColor Yellow
dotnet clean

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nFailed to clean!" -ForegroundColor Red
    exit 1
}

# Restore
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nFailed to restore packages!" -ForegroundColor Red
    exit 1
}

# Build
Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build -c Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nRebuild completed successfully!" -ForegroundColor Green
    Write-Host "`nTo run the application, use:" -ForegroundColor Cyan
    Write-Host "  dotnet run" -ForegroundColor White
} else {
    Write-Host "`nRebuild failed!" -ForegroundColor Red
    exit 1
}

