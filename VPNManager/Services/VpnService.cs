using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using VPNManager.Models;

namespace VPNManager.Services
{
    public class VpnService
    {
        private Process? _vpnProcess;
        private bool _isRunning;

        public event EventHandler<VpnStatus>? StatusChanged;

        public bool IsRunning => _isRunning;

        public void StartVpnCycle(AppSettings settings)
        {
            if (_isRunning)
                return;

            _isRunning = true;

            // Start PowerShell script with parameters
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VPN-AutoToggle.ps1");

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("VPN-AutoToggle.ps1 not found", scriptPath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = BuildPowerShellArguments(scriptPath, settings),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
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

        public void StopVpnCycle()
        {
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

            // Disconnect any active VPN connection
            DisconnectAllVpn();
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

        private void DisconnectAllVpn()
        {
            try
            {
                // Try to disconnect using rasdial
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
            catch (Exception)
            {
                // Ignore errors during disconnect
            }
        }

        private string BuildPowerShellArguments(string scriptPath, AppSettings settings)
        {
            var args = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -VpnName \"{settings.VpnName}\"";

            if (!string.IsNullOrEmpty(settings.Username))
            {
                args += $" -Username \"{settings.Username}\"";
            }

            if (!string.IsNullOrEmpty(settings.Password))
            {
                args += $" -Password \"{settings.Password}\"";
            }

            args += $" -CycleDurationMinutes {settings.CycleDurationMinutes}";
            args += $" -MaxRetries {settings.MaxRetries}";

            return args;
        }

        protected virtual void OnStatusChanged(VpnStatus status)
        {
            StatusChanged?.Invoke(this, status);
        }

        // Check if VPN is available on the system
        public bool IsVpnAvailable(string vpnName)
        {
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
