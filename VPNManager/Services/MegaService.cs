using System;
using System.Diagnostics;
using VPNManager.Models;

namespace VPNManager.Services
{
    public class MegaService
    {
        public MegaStatus GetCurrentStatus()
        {
            var status = new MegaStatus
            {
                LastChecked = DateTime.Now
            };

            try
            {
                var processes = Process.GetProcessesByName("MEGAsync");

                if (processes.Length > 0)
                {
                    status.IsRunning = true;
                    status.ProcessId = processes[0].Id;
                    status.ProcessName = "MEGAsync";
                    status.Status = "Running";

                    // Try to determine if it's syncing
                    // This is a simplified check - in reality you might need to check the MEGAsync API or window titles
                    status.IsSyncing = IsMegasyncSyncing(processes[0]);
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

        private bool IsMegasyncSyncing(Process process)
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

        public bool IsMegasyncInstalled()
        {
            try
            {
                var processes = Process.GetProcessesByName("MEGAsync");
                if (processes.Length > 0)
                {
                    foreach (var p in processes)
                    {
                        p.Dispose();
                    }
                    return true;
                }

                // Check if MEGAsync.exe exists in common locations
                var commonPaths = new[]
                {
                    @"C:\Program Files\MEGAsync\MEGAsync.exe",
                    @"C:\Program Files (x86)\MEGAsync\MEGAsync.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        @"MEGAsync\MEGAsync.exe")
                };

                foreach (var path in commonPaths)
                {
                    if (System.IO.File.Exists(path))
                        return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public string GetMegasyncVersion()
        {
            try
            {
                var commonPaths = new[]
                {
                    @"C:\Program Files\MEGAsync\MEGAsync.exe",
                    @"C:\Program Files (x86)\MEGAsync\MEGAsync.exe",
                };

                foreach (var path in commonPaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(path);
                        return versionInfo.FileVersion ?? "Unknown";
                    }
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
