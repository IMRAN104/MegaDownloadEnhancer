using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using VPNManager.Models;

namespace VPNManager.Services
{
    public class VpnService
    {
        private Process? _vpnProcess;
        private bool _isRunning;
        private AppSettings _settings;

        public event EventHandler<VpnStatus>? StatusChanged;

        public bool IsRunning => _isRunning;

        public VpnService(AppSettings settings)
        {
            _settings = settings;
        }

        public void StartVpnCycle(AppSettings settings)
        {
            if (_isRunning)
                return;

            _isRunning = true;

            // Create settings.json for the PowerShell script
            var settingsJsonPath = CreateSettingsJsonFile(settings);

            // Start PowerShell script
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VPN-AutoToggle.ps1");

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("VPN-AutoToggle.ps1 not found", scriptPath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -SettingsPath \"{settingsJsonPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            _vpnProcess = Process.Start(psi);

            if (_vpnProcess != null)
            {
                _vpnProcess.EnableRaisingEvents = true;
                _vpnProcess.Exited += (s, e) =>
                {
                    _isRunning = false;
                    OnStatusChanged(new VpnStatus
                    {
                        IsConnected = false,
                        CurrentState = "Stopped",
                        LastStateChange = DateTime.Now
                    });
                };
            }
        }

        private string CreateSettingsJsonFile(AppSettings settings)
        {
            // Determine if using WARP
            var useWarp = settings.VpnName.Equals("WARP", StringComparison.OrdinalIgnoreCase) ||
                          settings.VpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase);

            // Create settings object for PowerShell script
            var psSettings = new
            {
                VpnName = settings.VpnName,
                UseWarp = useWarp,
                CycleDurationMinutes = settings.CycleDurationMinutes,
                MaxRetries = settings.MaxRetries,
                MegasyncPath = FindMegasyncPath(),
                MegasyncRestartDelaySeconds = 5,
                WarpUiPath = @"C:\Program Files\Cloudflare\Cloudflare WARP\Cloudflare WARP.exe",
                LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VPN-AutoToggle.log")
            };

            // Save to a temporary file in the application directory
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vpn-settings.json");
            var json = System.Text.Json.JsonSerializer.Serialize(psSettings, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(settingsPath, json);

            return settingsPath;
        }

        private string FindMegasyncPath()
        {
            // Common MEGAsync paths
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MEGAsync\MEGAsync.exe"),
                @"C:\Program Files\MEGAsync\MEGAsync.exe",
                @"C:\Program Files (x86)\MEGAsync\MEGAsync.exe"
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }

            // Default fallback
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MEGAsync\MEGAsync.exe");
        }

        public void StopVpnCycle()
        {
            // First, disconnect VPN (this signals the PowerShell script to stop)
            DisconnectAllVpn();

            // Then kill the PowerShell process
            if (_vpnProcess != null && !_vpnProcess.HasExited)
            {
                try
                {
                    _vpnProcess.Kill(entireProcessTree: true);
                    _vpnProcess.WaitForExit(5000);
                }
                catch (Exception)
                {
                    // Process may have already exited
                }
            }

            _isRunning = false;
        }

        public VpnStatus GetCurrentStatus(string vpnName)
        {
            var status = new VpnStatus
            {
                VpnName = vpnName,
                LastStateChange = DateTime.Now
            };

            try
            {
                status.IsConnected = IsVpnConnected(vpnName);
                status.CurrentState = status.IsConnected ? "Connected" : "Disconnected";
                status.ConnectionName = vpnName;
            }
            catch (Exception ex)
            {
                status.LastError = ex.Message;
            }

            return status;
        }

        private bool IsVpnConnected(string vpnName)
        {
            // Check if it's WARP
            if (vpnName.Equals("WARP", StringComparison.OrdinalIgnoreCase) ||
                vpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase))
            {
                return IsWarpConnected();
            }

            // Standard Windows VPN check
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"Get-VpnConnection -Name \"{vpnName}\" | Select-Object -ExpandProperty ConnectionStatus",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    var output = process.StandardOutput.ReadToEnd();
                    return output.Trim().Equals("Connected", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private bool IsWarpConnected()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "warp-cli.exe",
                    Arguments = "status",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    var output = process.StandardOutput.ReadToEnd();
                    return output.Contains("Connected", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private void DisconnectAllVpn()
        {
            try
            {
                // Check if it's WARP
                var vpnName = _settings.VpnName;
                var isWarp = vpnName.Equals("WARP", StringComparison.OrdinalIgnoreCase) ||
                              vpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase);

                if (isWarp)
                {
                    // Disconnect WARP
                    var psi = new ProcessStartInfo
                    {
                        FileName = "warp-cli.exe",
                        Arguments = "disconnect",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    process?.WaitForExit(10000); // Wait up to 10 seconds for WARP to disconnect
                }
                else
                {
                    // Disconnect Windows VPN using rasdial
                    var psi = new ProcessStartInfo
                    {
                        FileName = "rasdial.exe",
                        Arguments = "/disconnect",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    process?.WaitForExit(5000);
                }
            }
            catch (Exception)
            {
                // Ignore errors during disconnect
            }
        }

        protected virtual void OnStatusChanged(VpnStatus status)
        {
            StatusChanged?.Invoke(this, status);
        }

        // Check if VPN is available on the system
        public bool IsVpnAvailable(string vpnName)
        {
            // Check for WARP
            if (vpnName.Equals("WARP", StringComparison.OrdinalIgnoreCase) ||
                vpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase))
            {
                return IsWarpAvailable();
            }

            // Standard Windows VPN check
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"Get-VpnConnection -Name \"{vpnName}\" -ErrorAction SilentlyContinue",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    return process.ExitCode == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private bool IsWarpAvailable()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "warp-cli.exe",
                    Arguments = "status",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    return process.ExitCode == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public string[] GetAvailableVpns()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Get-VpnConnection | Select-Object -ExpandProperty Name",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    var output = process.StandardOutput.ReadToEnd();
                    return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }

            return Array.Empty<string>();
        }
    }
}
