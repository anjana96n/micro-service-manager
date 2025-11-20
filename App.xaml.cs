using System;
using System.Windows;
using System.Windows.Threading;

namespace MicroserviceManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start application:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                              "Startup Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Unhandled exception:\n\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}",
                          "Error",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Critical error:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                              "Fatal Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}

