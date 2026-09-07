# Publish Script for Microservice Manager
# Creates ONE self-contained, compressed .exe (no loose DLLs, no .NET install required).

Write-Host "Publishing Microservice Manager..." -ForegroundColor Cyan

$publishDir = "bin\Release\net8.0-windows\win-x64\publish"
$exeName    = "MicroserviceManager.exe"

Write-Host "`nCreating single-file executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    exit 1
}

$srcExe = Join-Path $publishDir $exeName
if (-not (Test-Path $srcExe)) {
    Write-Host "`nExpected output not found: $srcExe" -ForegroundColor Red
    exit 1
}

# Copy the single exe to .\dist for easy grabbing.
New-Item -ItemType Directory -Force -Path "dist" | Out-Null
Copy-Item $srcExe "dist\$exeName" -Force

$sizeMb = [math]::Round((Get-Item "dist\$exeName").Length / 1MB, 1)

Write-Host "`nPublish completed successfully!" -ForegroundColor Green
Write-Host "`nStandalone executable ($sizeMb MB):" -ForegroundColor Cyan
Write-Host "  $(Resolve-Path "dist\$exeName")" -ForegroundColor White
Write-Host "`nTo put it on your Desktop, run:" -ForegroundColor Cyan
Write-Host "  Copy-Item `"dist\$exeName`" ([Environment]::GetFolderPath('Desktop'))" -ForegroundColor White
Write-Host "`nThen just double-click it. No installation needed (Windows 10/11 x64)." -ForegroundColor Yellow
