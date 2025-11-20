# Publish Script for Microservice Manager
# This script creates a standalone executable

Write-Host "Publishing Microservice Manager..." -ForegroundColor Cyan

# Create standalone executable
Write-Host "`nCreating standalone executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if ($LASTEXITCODE -eq 0) {
    $exePath = "bin\Release\net6.0-windows\win-x64\publish\MicroserviceManager.exe"
    
    Write-Host "`nPublish completed successfully!" -ForegroundColor Green
    Write-Host "`nExecutable created at:" -ForegroundColor Cyan
    Write-Host "  $exePath" -ForegroundColor White
    Write-Host "`nYou can now run the application by double-clicking the .exe file" -ForegroundColor Yellow
    Write-Host "or copy it to any location on your computer." -ForegroundColor Yellow
    
    # Calculate file size
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length / 1MB
        Write-Host "`nFile size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
    }
} else {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    exit 1
}

