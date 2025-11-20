# Quick Start Guide

## 1. Install Prerequisites

Make sure you have .NET 6.0 SDK installed:
```powershell
# Check if .NET is installed
dotnet --version
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/6.0

## 2. Build and Run

```powershell
# Navigate to the project directory
cd D:\My-Projects\Micro-run\MicroserviceManager

# Restore dependencies and build
dotnet restore
dotnet build

# Run the application
dotnet run
```

## 3. Add Your First Service

1. Click **➕ Add Service**
2. Enter:
   - **Name**: `Authorization Service`
   - **Path**: `D:\Projects\Services\authorization-service`
   - **Command**: `mvn spring-boot:run`
3. Click **Save**

## 4. Start Your Service

Click the **▶ Start** button next to your service and watch the console output!

## 5. Create Executable (Optional)

To create a standalone .exe file:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Find your .exe at: `bin\Release\net6.0-windows\win-x64\publish\MicroserviceManager.exe`

---

That's it! You're ready to manage your microservices! 🚀

