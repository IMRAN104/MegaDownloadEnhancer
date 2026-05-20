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
        private bool _isRunning;
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

            _isRunning = true;

            var settingsJsonPath = CreateSettingsJsonFile(settings);

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
            var useWarp = settings.VpnName.Equals("CloudflareWARP", StringComparison.OrdinalIgnoreCase);

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
            DisconnectAllVpn();

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
                        return output.Contains("Connected", StringComparison.OrdinalIgnoreCase);
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

        public void Dispose()
        {
            StopVpnCycle();
            DisposeProcess();
        }
    }
}
