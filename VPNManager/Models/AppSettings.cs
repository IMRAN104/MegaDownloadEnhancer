using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VPNManager.Models
{
    public class AppSettings
    {
        public string VpnName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// DPAPI-encrypted password (base64). Do not set manually.
        /// </summary>
        public string EncryptedPassword { get; set; } = string.Empty;
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
                    if (settings != null)
                    {
                        // Decrypt password from encrypted storage
                        settings.Password = DecryptPassword(settings.EncryptedPassword);
                        return settings;
                    }
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

                // Encrypt password before saving (Password is [JsonIgnore])
                if (!string.IsNullOrEmpty(Password))
                {
                    EncryptedPassword = EncryptPassword(Password);
                }
                else
                {
                    EncryptedPassword = string.Empty;
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

        /// <summary>
        /// Encrypts a password using Windows DPAPI (CurrentUser scope).
        /// The encrypted data is only decryptable by the same Windows user account.
        /// </summary>
        private static string EncryptPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts a DPAPI-encrypted password.
        /// </summary>
        private static string DecryptPassword(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return string.Empty;
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
