# Project Structure

## Overview

Microservice Manager is a WPF (Windows Presentation Foundation) desktop application built with .NET 8.0 for managing multiple Java microservices.

## Directory Structure

```
MicroserviceManager/
├── Models/
│   └── ServiceConfig.cs          # Data model for service configuration
├── Services/
│   └── ConfigManager.cs          # Handles saving/loading configurations
├── App.xaml                      # Application resources and startup
├── App.xaml.cs                   # Application code-behind
├── MainWindow.xaml               # Main application UI
├── MainWindow.xaml.cs            # Main window logic and process management
├── ServiceDialog.xaml            # Add/Edit service dialog UI
├── ServiceDialog.xaml.cs         # Dialog logic
├── MicroserviceManager.csproj    # Project file
├── MicroserviceManager.sln       # Visual Studio solution file
├── build.ps1                     # Build script
├── publish.ps1                   # Publish script (creates .exe)
├── run.ps1                       # Run script
├── .gitignore                    # Git ignore file
├── services.example.json         # Example configuration
├── README.md                     # Main documentation
├── QUICK_START.md                # Quick start guide
└── PROJECT_STRUCTURE.md          # This file
```

## Architecture

### Models (`Models/ServiceConfig.cs`)
- **ServiceConfig**: Represents a microservice configuration
  - `Name`: Service display name
  - `Path`: Project directory path
  - `Command`: Command to run the service
  - `IsRunning`: Current running status
  - `ProcessId`: Process ID when running

### Services (`Services/ConfigManager.cs`)
- **ConfigManager**: Manages persistent configuration
  - `SaveServices()`: Saves services to JSON file
  - `LoadServices()`: Loads services from JSON file
  - Configuration stored at: `%APPDATA%\MicroserviceManager\services.json`

### UI Components

#### MainWindow (`MainWindow.xaml` / `MainWindow.xaml.cs`)
Main application window with:
- **DataGrid**: Displays list of configured services
- **Action Buttons**: Add, Edit, Remove, Start All, Stop All
- **Console Output**: Real-time output from running services
- **Process Management**: Handles starting/stopping services

Key Methods:
- `StartService()`: Starts a microservice process
- `StopService()`: Stops a running service
- `KillProcessAndChildren()`: Terminates process tree
- `LogToConsole()`: Logs messages to console output

#### ServiceDialog (`ServiceDialog.xaml` / `ServiceDialog.xaml.cs`)
Dialog for adding/editing services:
- Service name input
- Project path selection with browse button
- Custom command configuration
- Input validation

### WPF Converters
Used for data binding in the UI:
- **StatusConverter**: Converts boolean to "Running"/"Stopped" text
- **StatusColorConverter**: Converts boolean to color codes
- **InverseBoolConverter**: Inverts boolean values for button enabling

## Data Flow

1. **Application Startup**:
   - Load configuration from `%APPDATA%\MicroserviceManager\services.json`
   - Populate DataGrid with services
   - Reset all running statuses

2. **Adding/Editing Service**:
   - Open ServiceDialog
   - Validate inputs
   - Save to ObservableCollection
   - Persist to JSON file

3. **Starting Service**:
   - Create Process with cmd.exe
   - Set working directory to project path
   - Execute configured command
   - Redirect stdout/stderr to console
   - Track process in dictionary

4. **Stopping Service**:
   - Find process in tracking dictionary
   - Execute `taskkill /T /F` to kill process tree
   - Update service status
   - Remove from tracking dictionary

5. **Application Shutdown**:
   - Check for running services
   - Prompt user to stop services
   - Optionally stop all before exit

## Key Technologies

- **WPF**: Modern Windows UI framework
- **MVVM Pattern**: Observable collections and data binding
- **.NET Process**: Managing external processes
- **Newtonsoft.Json**: JSON serialization
- **cmd.exe**: Shell for executing commands

## Process Management

The application uses Windows `cmd.exe` to execute commands:
```csharp
cmd.exe /c mvn spring-boot:run
```

Benefits:
- Inherits PATH environment variables
- Supports all command types (mvn, gradle, java, etc.)
- Proper working directory handling

The application captures:
- Standard Output (stdout)
- Standard Error (stderr)
- Process Exit events

## Configuration File Format

```json
[
  {
    "Name": "Service Name",
    "Path": "D:\\path\\to\\project",
    "Command": "mvn spring-boot:run",
    "IsRunning": false,
    "ProcessId": 0
  }
]
```

## UI Design Principles

- **Color Coding**:
  - Green: Start/Success actions
  - Red: Stop/Remove actions
  - Blue: Add/Edit actions
  - Gray: Neutral actions

- **Status Indicators**:
  - Running services: Green text
  - Stopped services: Gray text

- **Responsive Design**:
  - Buttons disable/enable based on state
  - Cannot edit/remove running services
  - Start button disabled when running
  - Stop button disabled when stopped

## Extension Points

### Adding New Features

1. **Custom Environment Variables**:
   - Extend `ServiceConfig` model
   - Add UI fields in `ServiceDialog`
   - Apply in `ProcessStartInfo.EnvironmentVariables`

2. **Service Groups**:
   - Add `Group` property to `ServiceConfig`
   - Implement group filtering in UI
   - Add "Start Group" functionality

3. **Service Dependencies**:
   - Add `Dependencies` list to `ServiceConfig`
   - Implement dependency checking before start
   - Auto-start dependencies

4. **Port Management**:
   - Add `Port` property to `ServiceConfig`
   - Check port availability before start
   - Display port in DataGrid

5. **Log File Export**:
   - Add "Export Logs" button
   - Save console output to file
   - Separate logs per service

## Build Process

### Development Build
```powershell
dotnet build
```
Creates debug build in `bin\Debug\`

### Release Build
```powershell
dotnet build -c Release
```
Creates optimized build in `bin\Release\`

### Standalone Executable
```powershell
.\publish.ps1
```
Creates ONE compressed, self-contained `dist\MicroserviceManager.exe` with the .NET runtime
and the native WPF libraries embedded (`IncludeNativeLibrariesForSelfExtract`). No loose DLLs.

## Dependencies

- **.NET 8.0 Runtime**: Windows desktop runtime (embedded in the standalone build)
- **Newtonsoft.Json**: JSON serialization (NuGet package)

## Platform Requirements

- **OS**: Windows 7 SP1 or higher
- **Architecture**: x64
- **.NET**: 6.0 or higher (included in standalone build)

## Future Enhancements

- [ ] Service health checks
- [ ] Restart on failure
- [ ] Custom environment variables per service
- [ ] Service profiles (dev, test, prod)
- [ ] Log filtering and search
- [ ] Service startup order/dependencies
- [ ] System tray support
- [ ] Auto-start on Windows login
- [ ] Service templates
- [ ] Import/Export configurations

