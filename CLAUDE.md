# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A Windows-only WPF desktop app (.NET 8) that launches and monitors local Java/Spring Boot
microservices as child processes. Single project, no solution-level split.

## Commands

- Build (Debug): `dotnet build`
- Build (Release): `dotnet build -c Release` or `.\build.ps1`
- Clean rebuild: `.\rebuild.ps1` (clean + restore + Debug build)
- Run the app: `dotnet run` or `.\run.ps1`
- Standalone single-file exe: `.\publish.ps1` → one compressed self-contained
  `bin\Release\net8.0-windows\win-x64\publish\MicroserviceManager.exe` (~69 MB), also copied
  to `dist\`. Flags that matter: `PublishSingleFile` + `IncludeNativeLibrariesForSelfExtract`
  (bundles the WPF `*_cor3.dll` natives) + `EnableCompressionInSingleFile` + `DebugType=none`.

There is no test project. The target framework is `net8.0-windows` and the app uses WPF +
WinForms interop, so it only builds and runs on Windows.

## Architecture

- **Not MVVM.** Effectively all application logic lives in `MainWindow.xaml.cs` code-behind.
  `services` is an `ObservableCollection<ServiceConfig>` bound to `ServicesDataGrid`;
  `runningProcesses` (`Dictionary<ServiceConfig, Process>`) tracks live processes.
- **Process model.** Each service is started as `cmd.exe /c <Command>` with
  `WorkingDirectory = service.Path`. stdout/stderr are redirected and pushed (via
  `Dispatcher.Invoke`) into `AddLog(service, text, isError)`. Stopping runs
  `taskkill /PID <pid> /T /F` to kill the whole tree (mvn/gradle spawn child JVMs).
- **Log store.** `AddLog` appends a `Models/LogEntry` to an in-memory `LinkedList` and keeps
  a per-service rolling cap (`MaxLinesPerService`, 5000). The console `TextBox` is only a
  rendered view: `AppendLine` for the fast incremental path, `RefreshConsole` for a full
  rebuild after filter changes / trims. `FilterCombo` picks which service's lines show
  (`currentFilter == null` = all); an optional `DispatcherTimer` (`autoClearTimer`) wipes the
  store on an interval set in `AutoClearCombo`.
- **Persistence.** `Services/ConfigManager.cs` serializes the service list to
  `%APPDATA%\MicroserviceManager\services.json` and UI prefs (`AutoClearMinutes`, `LastFilter`)
  to `settings.json` in the same folder, with Newtonsoft.Json. `IsRunning` / `ProcessId` are
  serialized but reset on load. `services.example.json` in the repo is only a sample and is
  never read by the app.
- **Editing model.** `ServiceDialog` operates on a *copy* of `ServiceConfig`; on save the
  collection item is replaced by index. Running services cannot be edited or removed.
- **Converters** (`StatusConverter`, `StatusColorConverter`, `InverseBoolConverter`) are
  defined at the bottom of `MainWindow.xaml.cs`.
- `MainWindow_Closing` prompts to stop still-running services before exit.

## Gotchas

- `ServiceConfig` (Models/) does **not** implement `INotifyPropertyChanged`. UI updates after
  mutating a service rely on manual `ServicesDataGrid.Items.Refresh()` calls — replicate that
  pattern for any new bound mutable field.
- The constructor sets combo boxes while `initializing == true`; the `*_Changed` handlers
  early-return on that flag so restoring saved settings doesn't overwrite them. Keep that
  guard when adding new persisted controls.
- `process.Exited` fires for the `cmd.exe` wrapper, not the underlying service process.
- Config lives in per-user `%APPDATA%`, not in the repo.
