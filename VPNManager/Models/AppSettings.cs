using System;
using System.IO;
using System.Text.Json;

namespace VPNManager.Models
{
    public class AppSettings
    {
        public string VpnName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int CycleDurationMinutes { get; set; } = 10;
        public int MaxRetries { get; set; } = 3;
        public bool MinimizeToTray { get; set; } = true;
        public bool StartMinimized { get; set; } = false;
        public int StatusRefreshIntervalSeconds { get; set; } = 1;
        public string MonitoredProcessName { get; set; } = "MEGAsync";
        public string MonitoredProcessDisplayName { get; set; } = "MEGAsync";
        public bool ProcessMonitoringEnabled { get; set; } = true;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VPNManager",
            "settings.json"
        );

        public static AppSettings Load()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                // If loading fails, return default settings
            }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception)
            {
                // Handle save error silently or log it
            }
        }
    }
}
