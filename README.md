# Microservice Manager

A Windows desktop application for managing and running multiple Java/Spring Boot microservices locally.

## Features

- ✅ Configure multiple microservice projects with custom paths
- ✅ Save service configurations persistently
- ✅ Customize run commands for each service (default: `mvn spring-boot:run`)
- ✅ Start/Stop individual services or all services at once
- ✅ Real-time console output for all running services
- ✅ Track service status (Running/Stopped)
- ✅ Modern, user-friendly interface

## Prerequisites

- .NET 6.0 SDK or higher ([Download here](https://dotnet.microsoft.com/download/dotnet/6.0))
- Java Development Kit (JDK) for your Spring Boot projects
- Maven (if using `mvn` commands)

## Building the Application

1. Open Command Prompt or PowerShell
2. Navigate to the project directory:
   ```powershell
   cd D:\My-Projects\Micro-run\MicroserviceManager
   ```

3. Build the application:
   ```powershell
   dotnet build
   ```

4. Run the application:
   ```powershell
   dotnet run
   ```

## Creating a Standalone Executable

To create a standalone `.exe` file that doesn't require .NET SDK to be installed:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be created in:
```
bin\Release\net6.0-windows\win-x64\publish\MicroserviceManager.exe
```

## How to Use

### Adding a New Service

1. Click the **➕ Add Service** button
2. Fill in the service details:
   - **Service Name**: A friendly name for your service (e.g., "Authorization Service")
   - **Project Path**: Full path to your project directory (e.g., `D:\Projects\Services\authorization-service`)
   - **Run Command**: The command to run your service (e.g., `mvn spring-boot:run`, `gradlew bootRun`, or `java -jar target/app.jar`)
3. Click **Save**

### Editing a Service

1. Select a service from the list
2. Click the **✏️ Edit Service** button
3. Modify the details
4. Click **Save**

Note: You cannot edit a service while it's running. Stop it first.

### Removing a Service

1. Select a service from the list
2. Click the **🗑️ Remove Service** button
3. Confirm the deletion

Note: You cannot remove a service while it's running. Stop it first.

### Starting Services

**Start a single service:**
- Click the **▶ Start** button next to the service

**Start all services:**
- Click the **▶ Start All** button at the bottom

### Stopping Services

**Stop a single service:**
- Click the **⬛ Stop** button next to the service

**Stop all services:**
- Click the **⬛ Stop All** button at the bottom

### Monitoring Output

- All console output from running services appears in the **Console Output** section at the bottom
- Each log line is prefixed with a timestamp and service name
- Use the **Clear** button to clear the console output

## Configuration Storage

Service configurations are automatically saved to:
```
%APPDATA%\MicroserviceManager\services.json
```

This ensures your configurations persist between application restarts.

## Common Commands

Here are some common commands you might use for different project types:

### Maven Projects
```
mvn spring-boot:run
mvn spring-boot:run -Dspring-boot.run.profiles=dev
```

### Gradle Projects
```
gradlew bootRun
gradlew bootRun --args='--spring.profiles.active=dev'
```

### Executable JAR
```
java -jar target/myapp-1.0.0.jar
java -jar target/myapp-1.0.0.jar --spring.profiles.active=dev
```

### Custom Maven Goals
```
mvn clean install spring-boot:run
```

## Tips

- **Working Directory**: The command runs in the project path you specified, so relative paths in your command will work correctly
- **Environment Variables**: The application inherits all environment variables from Windows
- **Port Conflicts**: Make sure your services use different ports to avoid conflicts
- **Memory**: Each service runs in its own process, so ensure your system has sufficient memory
- **Logs**: Check the console output for errors if a service fails to start

## Troubleshooting

### Service won't start
- Verify the project path exists and is correct
- Ensure Maven/Gradle is in your system PATH
- Check if the port is already in use
- Look for errors in the console output

### "Process exited" immediately
- The command might have syntax errors
- Maven/Gradle might not be found (check PATH)
- The project might have compilation errors

### Changes to pom.xml/build.gradle not taking effect
- Stop the service and start it again
- Some commands might need `clean` before running (e.g., `mvn clean spring-boot:run`)

## Example Configuration

Here's an example configuration for a typical microservice setup:

| Service Name | Project Path | Command |
|--------------|--------------|---------|
| Gateway Service | D:\Projects\gateway | mvn spring-boot:run |
| Auth Service | D:\Projects\auth-service | mvn spring-boot:run -Dspring-boot.run.profiles=local |
| User Service | D:\Projects\user-service | gradlew bootRun |
| Product Service | D:\Projects\product-service | mvn spring-boot:run |

## License

Free to use and modify for your projects.

## Support

For issues or questions, feel free to open an issue or modify the code to suit your needs.

