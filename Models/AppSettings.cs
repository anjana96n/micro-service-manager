namespace MicroserviceManager.Models
{
    /// <summary>
    /// User preferences persisted alongside the service list, at
    /// %APPDATA%\MicroserviceManager\settings.json.
    /// </summary>
    public class AppSettings
    {
        /// <summary>Minutes between automatic console clears. 0 (or less) disables it.</summary>
        public int AutoClearMinutes { get; set; }

        /// <summary>Last selected console filter ("All services" or a service name / "System").</summary>
        public string LastFilter { get; set; } = "All services";
    }
}
