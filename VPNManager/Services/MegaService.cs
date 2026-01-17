using System;
using System.Diagnostics;
using System.IO;
using VPNManager.Models;

namespace VPNManager.Services
{
    public class MegaService
    {
        private readonly AppSettings _settings;

        public MegaService(AppSettings settings)
        {
            _settings = settings;
        }

        public MegaStatus GetCurrentStatus()
        {
            var status = new MegaStatus
            {
                LastChecked = DateTime.Now
            };

            try
            {
                if (!_settings.ProcessMonitoringEnabled)
                {
                    status.IsRunning = false;
                    status.Status = "Monitoring Disabled";
                    return status;
                }

                var processName = _settings.MonitoredProcessName;
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length > 0)
                {
                    status.IsRunning = true;
                    status.ProcessId = processes[0].Id;
                    status.ProcessName = _settings.MonitoredProcessDisplayName;
                    status.Status = "Running";

                    // Try to determine if it's syncing
                    status.IsSyncing = IsProcessSyncing(processes[0]);
                    status.Status = status.IsSyncing ? "Syncing" : "Idle";
                }
                else
                {
                    status.IsRunning = false;
                    status.Status = "Not Running";
                }
            }
            catch (Exception)
            {
                status.IsRunning = false;
                status.Status = "Error";
            }

            return status;
        }

        private bool IsProcessSyncing(Process process)
        {
            try
            {
                // Check window title or main window title for sync indicators
                var mainWindowTitle = process.MainWindowTitle;
                if (!string.IsNullOrEmpty(mainWindowTitle))
                {
                    return mainWindowTitle.Contains("Syncing", StringComparison.OrdinalIgnoreCase) ||
                           mainWindowTitle.Contains("Transferring", StringComparison.OrdinalIgnoreCase) ||
                           mainWindowTitle.Contains("Uploading", StringComparison.OrdinalIgnoreCase) ||
                           mainWindowTitle.Contains("Downloading", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return false;
        }

        public bool IsMonitoredProcessInstalled()
        {
            try
            {
                var processName = _settings.MonitoredProcessName;
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    foreach (var p in processes)
                    {
                        p.Dispose();
                    }
                    return true;
                }

                // Check if process exists in common locations
                var commonPaths = GetCommonProcessPaths();

                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                        return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public string FindExecutablePath()
        {
            var commonPaths = GetCommonProcessPaths();

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        private string[] GetCommonProcessPaths()
        {
            var processName = _settings.MonitoredProcessName;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            return new[]
            {
                Path.Combine(programFiles, "MEGAsync", "MEGAsync.exe"),
                Path.Combine(programFilesX86, "MEGAsync", "MEGAsync.exe"),
                Path.Combine(localAppData, "MEGAsync", "MEGAsync.exe"),
                Path.Combine(programFiles, processName, $"{processName}.exe"),
                Path.Combine(programFilesX86, processName, $"{processName}.exe"),
                Path.Combine(localAppData, processName, $"{processName}.exe"),
            };
        }

        public string GetProcessVersion()
        {
            try
            {
                var executablePath = FindExecutablePath();
                if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
                    return versionInfo.FileVersion ?? "Unknown";
                }
            }
            catch (Exception)
            {
                return "Unknown";
            }

            return "Not Installed";
        }
    }
}
