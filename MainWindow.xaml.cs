using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using MicroserviceManager.Models;
using MicroserviceManager.Services;

namespace MicroserviceManager
{
    public partial class MainWindow : Window
    {
        private const string AllServicesLabel = "All services";
        private const string SystemLabel = "System";

        // Rolling buffer: keep at most this many lines per service in memory so a chatty
        // service can never grow the console without bound and freeze the UI.
        private const int MaxLinesPerService = 5000;
        private const int TrimSlack = 256; // only trim once a service is this far over the cap

        private ObservableCollection<ServiceConfig> services = new ObservableCollection<ServiceConfig>();
        private Dictionary<ServiceConfig, Process> runningProcesses = new Dictionary<ServiceConfig, Process>();

        private readonly LinkedList<LogEntry> logEntries = new LinkedList<LogEntry>();
        private readonly Dictionary<string, int> perServiceCount = new Dictionary<string, int>();
        private string? currentFilter; // null => show everything

        private readonly DispatcherTimer autoClearTimer = new DispatcherTimer();
        private AppSettings settings = new AppSettings();
        private bool initializing = true;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                ServicesDataGrid.ItemsSource = services;

                settings = ConfigManager.LoadSettings();

                LoadServices();

                autoClearTimer.Tick += AutoClearTimer_Tick;

                // Restore console filter + auto-clear interval from saved settings.
                RebuildFilterItems();
                currentFilter = settings.LastFilter == AllServicesLabel ? null : settings.LastFilter;
                FilterCombo.SelectedItem = settings.LastFilter;

                AutoClearCombo.Text = settings.AutoClearMinutes > 0
                    ? settings.AutoClearMinutes.ToString(CultureInfo.InvariantCulture)
                    : "Off";
                ApplyAutoClearInterval();

                Closing += MainWindow_Closing;

                initializing = false;

                AddLog(SystemLabel, "Microservice Manager started successfully.");
            }
            catch (Exception ex)
            {
                initializing = false;
                MessageBox.Show($"Error initializing application: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                              "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                var loadedServices = ConfigManager.LoadServices();
                services.Clear();
                foreach (var service in loadedServices)
                {
                    service.IsRunning = false; // Reset running status
                    services.Add(service);
                }
                RebuildFilterItems();
                AddLog(SystemLabel, "Services loaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveServices()
        {
            try
            {
                ConfigManager.SaveServices(services.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            if (initializing) return;
            try
            {
                ConfigManager.SaveSettings(settings);
            }
            catch
            {
                // Persisting preferences is best-effort; don't interrupt the user for it.
            }
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ServiceDialog
                {
                    Owner = this
                };
                var result = dialog.ShowDialog();

                if (result == true && dialog.Service != null)
                {
                    services.Add(dialog.Service);
                    SaveServices();
                    RebuildFilterItems();
                    ServicesDataGrid.Items.Refresh();
                    AddLog(SystemLabel, $"Added service: {dialog.Service.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding service: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ServicesDataGrid.SelectedItem is ServiceConfig selectedService)
                {
                    if (selectedService.IsRunning)
                    {
                        MessageBox.Show("Cannot edit a running service. Please stop it first.", "Warning",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dialog = new ServiceDialog(selectedService)
                    {
                        Owner = this
                    };
                    if (dialog.ShowDialog() == true && dialog.Service != null)
                    {
                        var index = services.IndexOf(selectedService);
                        services[index] = dialog.Service;
                        SaveServices();
                        RebuildFilterItems();
                        ServicesDataGrid.Items.Refresh();
                        AddLog(SystemLabel, $"Updated service: {dialog.Service.Name}");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a service to edit.", "Information",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing service: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem is ServiceConfig selectedService)
            {
                if (selectedService.IsRunning)
                {
                    MessageBox.Show("Cannot remove a running service. Please stop it first.", "Warning",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to remove '{selectedService.Name}'?",
                                            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    services.Remove(selectedService);
                    SaveServices();
                    RebuildFilterItems();
                    AddLog(SystemLabel, $"Removed service: {selectedService.Name}");
                }
            }
            else
            {
                MessageBox.Show("Please select a service to remove.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StartService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServiceConfig service)
            {
                StartService(service);
            }
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServiceConfig service)
            {
                StopService(service);
            }
        }

        private void StartAll_Click(object sender, RoutedEventArgs e)
        {
            AddLog(SystemLabel, "Starting all services...");
            foreach (var service in services.Where(s => !s.IsRunning))
            {
                StartService(service);
            }
        }

        private void StopAll_Click(object sender, RoutedEventArgs e)
        {
            AddLog(SystemLabel, "Stopping all services...");
            foreach (var service in services.Where(s => s.IsRunning).ToList())
            {
                StopService(service);
            }
        }

        private void StartService(ServiceConfig service)
        {
            try
            {
                if (!Directory.Exists(service.Path))
                {
                    MessageBox.Show($"Project path does not exist: {service.Path}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {service.Command}",
                    WorkingDirectory = service.Path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = processInfo };

                process.OutputDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() => AddLog(service.Name, args.Data));
                    }
                };

                process.ErrorDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() => AddLog(service.Name, args.Data, isError: true));
                    }
                };

                process.Exited += (s, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        service.IsRunning = false;
                        runningProcesses.Remove(service);
                        ServicesDataGrid.Items.Refresh();
                        AddLog(service.Name, "Process exited.");
                    });
                };

                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                service.IsRunning = true;
                service.ProcessId = process.Id;
                runningProcesses[service] = process;
                ServicesDataGrid.Items.Refresh();

                AddLog(service.Name, $"Started service (PID: {process.Id})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting service '{service.Name}': {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                AddLog(service.Name, $"Failed to start: {ex.Message}", isError: true);
            }
        }

        private void StopService(ServiceConfig service)
        {
            try
            {
                if (runningProcesses.ContainsKey(service))
                {
                    var process = runningProcesses[service];

                    // Kill the process tree (including child processes)
                    KillProcessAndChildren(process.Id);

                    service.IsRunning = false;
                    runningProcesses.Remove(service);
                    ServicesDataGrid.Items.Refresh();

                    AddLog(service.Name, "Stopped service.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping service '{service.Name}': {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                AddLog(service.Name, $"Failed to stop: {ex.Message}", isError: true);
            }
        }

        private void KillProcessAndChildren(int pid)
        {
            try
            {
                var killer = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/PID {pid} /T /F",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                killer.Start();
                killer.WaitForExit();
            }
            catch
            {
                // Process might already be terminated
            }
        }

        // ----- Console / log handling -------------------------------------------------

        private void AddLog(string service, string message, bool isError = false)
        {
            if (string.IsNullOrEmpty(service)) service = SystemLabel;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Service = service,
                Text = message,
                IsError = isError
            };

            logEntries.AddLast(entry);
            perServiceCount[service] = perServiceCount.TryGetValue(service, out var c) ? c + 1 : 1;

            var newService = !FilterContains(service);
            TrimIfNeeded(service);

            if (newService)
            {
                RebuildFilterItems();
            }

            if (PassesFilter(entry))
            {
                AppendLine(entry);
            }
        }

        private bool PassesFilter(LogEntry entry) => currentFilter == null || entry.Service == currentFilter;

        private void TrimIfNeeded(string service)
        {
            if (perServiceCount.TryGetValue(service, out var count) && count > MaxLinesPerService + TrimSlack)
            {
                var toRemove = count - MaxLinesPerService;
                var node = logEntries.First;
                var removedVisible = false;
                while (node != null && toRemove > 0)
                {
                    var next = node.Next;
                    if (node.Value.Service == service)
                    {
                        if (PassesFilter(node.Value)) removedVisible = true;
                        logEntries.Remove(node);
                        toRemove--;
                    }
                    node = next;
                }
                perServiceCount[service] = MaxLinesPerService;
                if (removedVisible)
                {
                    RefreshConsole();
                }
            }
        }

        private void AppendLine(LogEntry entry)
        {
            try
            {
                ConsoleOutput.AppendText(entry.Format() + "\n");
                ConsoleOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Console append failed: {ex.Message}");
            }
        }

        private void RefreshConsole()
        {
            var sb = new StringBuilder();
            foreach (var entry in logEntries)
            {
                if (PassesFilter(entry))
                {
                    sb.Append(entry.Format()).Append('\n');
                }
            }
            ConsoleOutput.Text = sb.ToString();
            ConsoleOutput.ScrollToEnd();
        }

        private bool FilterContains(string service)
        {
            foreach (var item in FilterCombo.Items)
            {
                if (item as string == service) return true;
            }
            return false;
        }

        private void RebuildFilterItems()
        {
            var previous = FilterCombo.SelectedItem as string ?? currentFilter ?? AllServicesLabel;

            var names = new List<string> { AllServicesLabel, SystemLabel };
            foreach (var s in services)
            {
                if (!string.IsNullOrWhiteSpace(s.Name) && !names.Contains(s.Name))
                {
                    names.Add(s.Name);
                }
            }
            // Include any service that only exists in the logs (e.g. a since-renamed one).
            foreach (var svc in perServiceCount.Keys)
            {
                if (!names.Contains(svc)) names.Add(svc);
            }

            FilterCombo.ItemsSource = names;
            FilterCombo.SelectedItem = names.Contains(previous) ? previous : AllServicesLabel;
        }

        private void FilterCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (initializing) return;
            var selected = FilterCombo.SelectedItem as string ?? AllServicesLabel;
            currentFilter = selected == AllServicesLabel ? null : selected;
            settings.LastFilter = selected;
            SaveSettings();
            RefreshConsole();
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            if (currentFilter == null)
            {
                logEntries.Clear();
                perServiceCount.Clear();
            }
            else
            {
                var node = logEntries.First;
                while (node != null)
                {
                    var next = node.Next;
                    if (node.Value.Service == currentFilter) logEntries.Remove(node);
                    node = next;
                }
                perServiceCount.Remove(currentFilter);
            }
            RefreshConsole();
        }

        // ----- Auto-clear timer -----------------------------------------------------

        private void AutoClearCombo_Changed(object sender, RoutedEventArgs e)
        {
            if (initializing) return;
            ApplyAutoClearInterval();
        }

        private void ApplyAutoClearInterval()
        {
            var minutes = ParseMinutes(AutoClearCombo.Text);
            settings.AutoClearMinutes = minutes;
            SaveSettings();

            autoClearTimer.Stop();
            if (minutes > 0)
            {
                autoClearTimer.Interval = TimeSpan.FromMinutes(minutes);
                autoClearTimer.Start();
            }
        }

        private static int ParseMinutes(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            text = text.Trim();
            if (text.Equals("Off", StringComparison.OrdinalIgnoreCase)) return 0;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n > 0 ? n : 0;
        }

        private void AutoClearTimer_Tick(object? sender, EventArgs e)
        {
            logEntries.Clear();
            perServiceCount.Clear();
            RefreshConsole();
            AddLog(SystemLabel, "Logs auto-cleared.");
        }

        // -------------------------------------------------------------------------

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (runningProcesses.Any())
            {
                var result = MessageBox.Show(
                    "Some services are still running. Do you want to stop them before exiting?",
                    "Confirm Exit",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var service in services.Where(s => s.IsRunning).ToList())
                    {
                        StopService(service);
                    }
                }
            }

            autoClearTimer.Stop();
            SaveSettings();
        }
    }

    // Converters
    public class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Running" : "Stopped";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "#27AE60" : "#95A5A6";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
