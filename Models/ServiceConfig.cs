using System;

namespace MicroserviceManager.Models
{
    public class ServiceConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Command { get; set; } = "mvn spring-boot:run";
        public bool IsRunning { get; set; }
        public int ProcessId { get; set; }
    }
}

