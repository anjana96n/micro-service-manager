using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MicroserviceManager.Models;

namespace MicroserviceManager.Services
{
    public class ConfigManager
    {
        private static readonly string ConfigFilePath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                         "MicroserviceManager", "services.json");

        public static void SaveServices(List<ServiceConfig> services)
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(services, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        public static List<ServiceConfig> LoadServices()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return new List<ServiceConfig>();
                }

                var json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<List<ServiceConfig>>(json) ?? new List<ServiceConfig>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load configuration: {ex.Message}", ex);
            }
        }
    }
}

