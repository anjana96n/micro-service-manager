using System.Windows;
using MicroserviceManager.Models;
using WinForms = System.Windows.Forms;

namespace MicroserviceManager
{
    public partial class ServiceDialog : Window
    {
        public ServiceConfig Service { get; private set; } = new ServiceConfig();

        public ServiceDialog(ServiceConfig? existingService = null)
        {
            InitializeComponent();

            if (existingService != null)
            {
                Service = new ServiceConfig
                {
                    Name = existingService.Name,
                    Path = existingService.Path,
                    Command = existingService.Command
                };
                
                Title = "Edit Service";
                ServiceNameTextBox.Text = Service.Name;
                ProjectPathTextBox.Text = Service.Path;
                CommandTextBox.Text = Service.Command;
            }
            else
            {
                Service = new ServiceConfig();
                Title = "Add New Service";
                CommandTextBox.Text = "mvn spring-boot:run"; // Default command
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Select Project Folder",
                UseDescriptionForTitle = true,
                SelectedPath = ProjectPathTextBox.Text
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                ProjectPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(ServiceNameTextBox.Text))
            {
                MessageBox.Show("Please enter a service name.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                ServiceNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ProjectPathTextBox.Text))
            {
                MessageBox.Show("Please enter a project path.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                ProjectPathTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(CommandTextBox.Text))
            {
                MessageBox.Show("Please enter a run command.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CommandTextBox.Focus();
                return;
            }

            // Save service configuration
            Service.Name = ServiceNameTextBox.Text.Trim();
            Service.Path = ProjectPathTextBox.Text.Trim();
            Service.Command = CommandTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

