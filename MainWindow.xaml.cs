using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MicroserviceManager.Models;
using MicroserviceManager.Services;

namespace MicroserviceManager
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ServiceConfig> services = new ObservableCollection<ServiceConfig>();
        private Dictionary<ServiceConfig, Process> runningProcesses = new Dictionary<ServiceConfig, Process>();

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                // Set up data grid
                ServicesDataGrid.ItemsSource = services;
                
                // Load services
                LoadServices();

                // Handle window closing
                Closing += MainWindow_Closing;
                
                // Initial console message
                LogToConsole("Microservice Manager started successfully.");
            }
            catch (Exception ex)
            {
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
                LogToConsole("Services loaded successfully.");
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
                    ServicesDataGrid.Items.Refresh();
                    LogToConsole($"Added service: {dialog.Service.Name}");
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
                        ServicesDataGrid.Items.Refresh();
                        LogToConsole($"Updated service: {dialog.Service.Name}");
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
                    LogToConsole($"Removed service: {selectedService.Name}");
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
            LogToConsole("Starting all services...");
            foreach (var service in services.Where(s => !s.IsRunning))
            {
                StartService(service);
            }
        }

        private void StopAll_Click(object sender, RoutedEventArgs e)
        {
            LogToConsole("Stopping all services...");
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
                        Dispatcher.Invoke(() => LogToConsole($"[{service.Name}] {args.Data}"));
                    }
                };

                process.ErrorDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() => LogToConsole($"[{service.Name}] ERROR: {args.Data}"));
                    }
                };

                process.Exited += (s, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        service.IsRunning = false;
                        runningProcesses.Remove(service);
                        ServicesDataGrid.Items.Refresh();
                        LogToConsole($"[{service.Name}] Process exited.");
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
                
                LogToConsole($"Started service: {service.Name} (PID: {process.Id})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting service '{service.Name}': {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                LogToConsole($"Failed to start {service.Name}: {ex.Message}");
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
                    
                    LogToConsole($"Stopped service: {service.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping service '{service.Name}': {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                LogToConsole($"Failed to stop {service.Name}: {ex.Message}");
            }
        }

        private void KillProcessAndChildren(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                
                // Kill child processes first
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

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Clear();
        }

        private void LogToConsole(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                ConsoleOutput.AppendText($"[{timestamp}] {message}\n");
                ConsoleScrollViewer.ScrollToEnd();
            }
            catch (Exception ex)
            {
                // Fallback if console output fails
                System.Diagnostics.Debug.WriteLine($"Console logging failed: {ex.Message}");
            }
        }

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

