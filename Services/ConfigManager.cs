using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MicroserviceManager.Models;

namespace MicroserviceManager.Services
{
    public class ConfigManager
    {
        private static readonly string ConfigDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "MicroserviceManager");

        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "services.json");
        private static readonly string SettingsFilePath = Path.Combine(ConfigDirectory, "settings.json");

        public static void SaveServices(List<ServiceConfig> services)
        {
            try
            {
                EnsureDirectory();
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

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                EnsureDirectory();
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}", ex);
            }
        }

        public static AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                // A corrupt settings file should never block startup.
                return new AppSettings();
            }
        }

        private static void EnsureDirectory()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }
    }
}
