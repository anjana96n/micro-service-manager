# Getting Started with Microservice Manager 🚀

## ✅ What You Got

A complete Windows desktop application for managing your Java microservices with:

- ✅ **Visual Interface** - Modern WPF application with intuitive UI
- ✅ **Service Configuration** - Save your service paths and commands
- ✅ **One-Click Start** - Run individual or all services at once
- ✅ **Real-time Console** - Monitor all service output in one place
- ✅ **Process Management** - Start/stop services with proper cleanup
- ✅ **Persistent Settings** - Your configurations are saved automatically

## 🎯 Quick Start (3 Steps)

### Step 1: Run the Application

**Option A - From Visual Studio / Rider:**
1. Open `MicroserviceManager.sln`
2. Press F5 to run

**Option B - From Command Line:**
```powershell
cd D:\My-Projects\Micro-run\MicroserviceManager
dotnet run
```

**Option C - Using the Script:**
```powershell
cd D:\My-Projects\Micro-run\MicroserviceManager
.\run.ps1
```

### Step 2: Add Your Services

1. Click **➕ Add Service**
2. Fill in the details:
   ```
   Service Name: authorization-service
   Project Path: D:\Projects\Services\authorization-service
   Command: mvn spring-boot:run
   ```
3. Click **Save**

Repeat for all your microservices!

### Step 3: Start Your Services

- **Start One**: Click ▶ Start next to a service
- **Start All**: Click ▶ Start All button
- **Monitor**: Watch the console output for logs

## 📁 Project Structure

```
MicroserviceManager/
├── 📄 MicroserviceManager.sln       # Open this in Visual Studio
├── 📄 MicroserviceManager.csproj    # Project configuration
├── 📄 build.ps1                      # Build the application
├── 📄 publish.ps1                    # Create standalone .exe
├── 📄 run.ps1                        # Run the application
├── 📄 README.md                      # Full documentation
├── 📄 QUICK_START.md                 # Quick reference
├── 📄 PROJECT_STRUCTURE.md           # Technical details
└── 📄 services.example.json          # Example configuration
```

## 🔧 Common Commands for Your Services

### Maven Commands
```bash
mvn spring-boot:run
mvn spring-boot:run -Dspring-boot.run.profiles=dev
mvn spring-boot:run -Dspring-boot.run.profiles=local
mvn clean spring-boot:run
```

### Gradle Commands
```bash
gradlew bootRun
gradlew bootRun --args='--spring.profiles.active=dev'
./gradlew bootRun
```

### Java JAR Commands
```bash
java -jar target/app-1.0.0.jar
java -jar target/app.jar --spring.profiles.active=local
```

## 💡 Tips & Tricks

### Add Multiple Services Quickly
Add all your services once, and they'll be saved permanently. You never have to reconfigure them!

### Different Profiles
You can create separate entries for the same service with different profiles:
```
Service Name: Auth Service (Dev)
Command: mvn spring-boot:run -Dspring-boot.run.profiles=dev

Service Name: Auth Service (Local)
Command: mvn spring-boot:run -Dspring-boot.run.profiles=local
```

### Check Logs
All service output appears in the console at the bottom. Each line shows:
- **Timestamp**: When the message occurred
- **Service Name**: Which service produced it
- **Message**: The actual log output

### Stop Before Closing
The app will prompt you if services are running when you close. You can:
- Stop all services and exit
- Exit without stopping (services keep running)
- Cancel and go back

## 🎁 Create Standalone Executable

Want to run the app without installing .NET?

```powershell
cd D:\My-Projects\Micro-run\MicroserviceManager
.\publish.ps1
```

This creates one self-contained file: `dist\MicroserviceManager.exe` (~69 MB, no loose DLLs).

You can:
- Double-click to run
- Copy to your Desktop
- Share with your team
- Run on any Windows 10/11 machine (no installation needed!)

## 📝 Configuration Location

Your service configurations are saved at:
```
%APPDATA%\MicroserviceManager\services.json
```

On Windows, this is typically:
```
C:\Users\YourUsername\AppData\Roaming\MicroserviceManager\services.json
```

## 🆘 Troubleshooting

### "Project path does not exist"
- Make sure the path is correct
- Use the Browse button to select the folder
- The path should point to the root of your project (where pom.xml or build.gradle is)

### "Command not found"
- Make sure Maven/Gradle is in your PATH
- Open a new Command Prompt and try running `mvn --version`
- Restart the application after installing Maven/Gradle

### Service starts then immediately stops
- Check the console output for error messages
- Make sure the project compiles: `cd <path> && mvn clean install`
- Verify the command works in a regular terminal first

### Port already in use
- Make sure each service uses a different port
- Check if another instance is already running
- Stop the existing service first

## 🎨 UI Overview

```
┌─────────────────────────────────────────────────┐
│  Microservice Manager                           │
│  Manage and run your Spring Boot microservices │
├─────────────────────────────────────────────────┤
│ Services                                        │
│ ┌───────────────────────────────────────────┐  │
│ │ Name    │ Path  │ Command  │ Status │ Actions│ │
│ │ Service1│ D:\...│ mvn...  │Running│ ▶ ⬛  │ │
│ │ Service2│ D:\...│ mvn...  │Stopped│ ▶ ⬛  │ │
│ └───────────────────────────────────────────┘  │
│                                                  │
│ [➕ Add] [✏️ Edit] [🗑️ Remove] [▶ Start All] [⬛ Stop All]│
├─────────────────────────────────────────────────┤
│ Console Output                         [Clear]  │
│ ┌───────────────────────────────────────────┐  │
│ │ [12:34:56] [Service1] Starting...        │  │
│ │ [12:34:57] [Service1] Started on port... │  │
│ │ [12:35:01] [Service2] Initializing...    │  │
│ └───────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

## 📚 Example Configuration

Here's a complete example for a typical microservices setup:

| Service | Path | Command |
|---------|------|---------|
| API Gateway | `D:\Projects\api-gateway` | `mvn spring-boot:run` |
| Auth Service | `D:\Projects\authorization-service` | `mvn spring-boot:run -Dspring-boot.run.profiles=local` |
| User Service | `D:\Projects\user-service` | `mvn spring-boot:run` |
| Product Service | `D:\Projects\product-service` | `gradlew bootRun` |
| Order Service | `D:\Projects\order-service` | `java -jar target/order-service.jar` |

## 🚀 Next Steps

1. **Run it now**: `.\run.ps1` or `dotnet run`
2. **Add your services**: Click ➕ Add Service
3. **Start all**: Click ▶ Start All
4. **Create .exe**: Run `.\publish.ps1` for portable version

## 💬 Need Help?

- Check `README.md` for detailed documentation
- See `PROJECT_STRUCTURE.md` for technical details
- View `services.example.json` for configuration examples

---

**You're all set!** Enjoy managing your microservices! 🎉

