# Run Script for Microservice Manager
# This script runs the application directly

Write-Host "Starting Microservice Manager..." -ForegroundColor Cyan
Write-Host ""

dotnet run

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nApplication exited with errors!" -ForegroundColor Red
    exit 1
}

