using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VPNManager.Models;

namespace VPNManager.Services
{
    public class VpnService : IDisposable
    {
        private Process? _vpnProcess;
        private volatile bool _isRunning;
        private readonly AppSettings _settings;

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

            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VPN-AutoToggle.ps1");
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("VPN-AutoToggle.ps1 not found", scriptPath);

            var useWarp = settings.VpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase);
            var megasyncPath = FindMegasyncPath();
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VPN-AutoToggle.log");

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            // Use ArgumentList (array form) — never interpolate user values into a command string
            psi.ArgumentList.Add("-ExecutionPolicy"); psi.ArgumentList.Add("Bypass");
            psi.ArgumentList.Add("-NonInteractive");
            psi.ArgumentList.Add("-File"); psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add("-VpnName"); psi.ArgumentList.Add(settings.VpnName);
            if (useWarp) psi.ArgumentList.Add("-UseWarp");
            psi.ArgumentList.Add("-CycleDurationMinutes"); psi.ArgumentList.Add(settings.CycleDurationMinutes.ToString());
            psi.ArgumentList.Add("-MaxRetries"); psi.ArgumentList.Add(settings.MaxRetries.ToString());
            psi.ArgumentList.Add("-MegasyncPath"); psi.ArgumentList.Add(megasyncPath);
            psi.ArgumentList.Add("-MegasyncRestartDelaySeconds"); psi.ArgumentList.Add("5");
            psi.ArgumentList.Add("-LogPath"); psi.ArgumentList.Add(logPath);

            _vpnProcess = Process.Start(psi);

            if (_vpnProcess != null)
            {
                _isRunning = true;
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

        private string FindMegasyncPath()
        {
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

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MEGAsync\MEGAsync.exe");
        }

        public void StopVpnCycle()
        {
            // Kill the cycle script first — stops all toggling immediately
            if (_vpnProcess != null && !_vpnProcess.HasExited)
            {
                try
                {
                    _vpnProcess.Kill(entireProcessTree: true);
                    _vpnProcess.WaitForExit(5000);
                }
                catch (Exception)
                {
                }
            }

            DisposeProcess();
            _isRunning = false;

            // Disconnect VPN after script is dead so we leave a clean state
            DisconnectAllVpn();
        }

        private void DisposeProcess()
        {
            if (_vpnProcess != null)
            {
                try
                {
                    _vpnProcess.Dispose();
                }
                catch (Exception)
                {
                }
                _vpnProcess = null;
            }
        }

        public async Task<VpnStatus> GetCurrentStatusAsync(string vpnName)
        {
            var status = new VpnStatus
            {
                VpnName = vpnName,
                LastStateChange = DateTime.Now
            };

            try
            {
                status.IsConnected = await IsVpnConnectedAsync(vpnName);
                status.CurrentState = status.IsConnected ? "Connected" : "Disconnected";
                status.ConnectionName = vpnName;
            }
            catch (Exception ex)
            {
                status.LastError = ex.Message;
            }

            return status;
        }

        private async Task<bool> IsVpnConnectedAsync(string vpnName)
        {
            if (vpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase))
            {
                return await IsWarpConnectedAsync();
            }

            return await Task.Run(() =>
            {
                try
                {
                    // VPN name passed via env var — never interpolated into the command string
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    psi.ArgumentList.Add("-NonInteractive");
                    psi.ArgumentList.Add("-Command");
                    psi.ArgumentList.Add("Get-VpnConnection -Name $env:_VPN_NAME | Select-Object -ExpandProperty ConnectionStatus");
                    psi.Environment["_VPN_NAME"] = vpnName;

                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit(5000);
                        return output.Trim().Equals("Connected", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception)
                {
                }
                return false;
            });
        }

        private async Task<bool> IsWarpConnectedAsync()
        {
            return await Task.Run(() =>
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
                        // Must match "Status update: Connected" — NOT just "Connected"
                        // because "Disconnected" also contains the substring "Connected"
                        return output.Contains("Status update: Connected", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception)
                {
                }
                return false;
            });
        }

        private void DisconnectAllVpn()
        {
            try
            {
                var vpnName = _settings.VpnName;
                var isWarp = vpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase);

                if (isWarp)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "warp-cli.exe",
                        Arguments = "disconnect",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    process?.WaitForExit(10000);
                }
                else
                {
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
            }
        }

        protected virtual void OnStatusChanged(VpnStatus status)
        {
            StatusChanged?.Invoke(this, status);
        }

        public bool IsVpnAvailable(string vpnName)
        {
            if (vpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase))
            {
                return IsWarpAvailable();
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("-NonInteractive");
                psi.ArgumentList.Add("-Command");
                psi.ArgumentList.Add("Get-VpnConnection -Name $env:_VPN_NAME -ErrorAction SilentlyContinue");
                psi.Environment["_VPN_NAME"] = vpnName;

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
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(5000);
                    return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }

            return Array.Empty<string>();
        }

        public void Dispose()
        {
            if (_isRunning) StopVpnCycle();
            DisposeProcess();
        }
    }
}
