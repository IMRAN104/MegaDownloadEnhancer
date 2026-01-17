# VPN Auto-Toggle Script

[![Build Status](https://github.com/IMRAN104/MegaDownloadEnhancer/actions/workflows/build-test.yml/badge.svg)](https://github.com/IMRAN104/MegaDownloadEnhancer/actions/workflows/build-test.yml)
[![Latest Release](https://img.shields.io/github/v/release/IMRAN104/MegaDownloadEnhancer)](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/IMRAN104/MegaDownloadEnhancer/total)](https://github.com/IMRAN104/MegaDownloadEnhancer/releases)

Automated VPN connection toggler that cycles your VPN on and off at regular intervals. Perfect for scenarios requiring periodic IP rotation or VPN connection management.

## 📥 Download

**Latest Release:** [VPN Manager v1.2.0](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)

Download the Windows application with GUI or use the PowerShell script directly:

- **Windows App (Recommended):** Download `VPNManager-v1.2.0-win-x64.zip` from [Releases](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)
- **PowerShell Script:** Use `VPN-AutoToggle.ps1` from this repository

### Quick Install
1. [Download latest release](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)
2. Extract to folder (e.g., `C:\Program Files\VPNManager`)
3. Run `VPNManager.exe`
4. Configure settings and click "Start Cycle"

## 🎯 Features

- ✅ **Automatic VPN Cycling**: Toggle VPN on/off every N minutes (default: 10)
- ✅ **Robust Error Handling**: Automatic retry logic for failed connections
- ✅ **Comprehensive Logging**: Track all VPN state changes and errors
- ✅ **Graceful Shutdown**: Ctrl+C for clean exit
- ✅ **Real-time Status**: Live countdown and state display
- ✅ **Flexible Configuration**: Customizable timing, credentials, and retry settings
- ✅ **Windows Native**: Uses built-in Windows VPN management (rasdial)

## 📋 Requirements

- **OS**: Windows 10/11 or Windows Server 2016+
- **PowerShell**: Version 5.1 or higher
- **VPN**: Configured VPN connection in Windows
- **Permissions**: Administrator privileges recommended

## 🚀 Quick Start

### 1. Setup Your VPN
```powershell
# List your VPN connections
Get-VpnConnection
```

### 2. Run the Test Script
```powershell
# Verify your system is ready
.\Test-VpnSetup.ps1 -VpnName "YourVPNName"
```

### 3. Start Auto-Toggle
```powershell
# Basic usage (10-minute cycles)
.\VPN-AutoToggle.ps1 -VpnName "YourVPNName"

# Quick test (1-minute cycles)
.\VPN-AutoToggle.ps1 -VpnName "YourVPNName" -CycleDurationMinutes 1
```

## 📖 Documentation

- **[QUICK-START.md](QUICK-START.md)** - Get started in 3 steps
- **[SETUP-AND-USAGE.md](SETUP-AND-USAGE.md)** - Complete setup guide and troubleshooting

## 💡 Usage Examples

### Basic Usage
```powershell
# With saved credentials
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"
```

### With Credentials
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -Username "user@domain.com" -Password "YourPassword"
```

### Custom Timing
```powershell
# 5-minute cycles
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 5
```

### All Options
```powershell
.\VPN-AutoToggle.ps1 `
    -VpnName "MyVPN" `
    -Username "user@domain.com" `
    -Password "SecurePass" `
    -CycleDurationMinutes 10 `
    -LogPath "C:\Logs\vpn.log" `
    -MaxRetries 3
```

## 📁 Project Files

| File | Description |
|------|-------------|
| `VPNManager.exe` | Windows GUI application (from releases) |
| `VPN-AutoToggle.ps1` | Main automation script |
| `Test-VpnSetup.ps1` | System validation and testing tool |
| `QUICK-START.md` | Quick reference guide |
| `SETUP-AND-USAGE.md` | Detailed documentation |
| `CI_CD_GUIDE.md` | Automated CI/CD process guide |
| `readme.md` | This file |

## 🔧 Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `VpnName` | ✅ Yes | - | Name of your VPN connection |
| `Username` | ❌ No | - | VPN username (if not saved) |
| `Password` | ❌ No | - | VPN password (if not saved) |
| `CycleDurationMinutes` | ❌ No | 10 | Minutes for each state |
| `LogPath` | ❌ No | `.\VPN-AutoToggle.log` | Log file location |
| `MaxRetries` | ❌ No | 3 | Connection retry attempts |

## 🛑 Stopping the Script

Press `Ctrl+C` at any time for graceful shutdown. The script will:
- Complete the current operation safely
- Log the shutdown event
- Exit cleanly

## 🧪 Testing

Run the included test script to verify your setup:

```powershell
.\Test-VpnSetup.ps1 -VpnName "YourVPNName"
```

This checks:
- PowerShell version
- Administrator privileges
- Execution policy
- VPN module availability
- VPN connection existence
- rasdial command
- Script files

## 📊 Sample Output

```
========================================
   VPN Auto-Toggle Script v1.0
========================================

[2024-01-15 10:30:00] [Info] Script started with parameters:
[2024-01-15 10:30:00] [Info]   VPN Name: MyVPN
[2024-01-15 10:30:00] [Info]   Cycle Duration: 10 minutes
[2024-01-15 10:30:01] [Info] Attempting to connect to VPN 'MyVPN' (Attempt 1 of 3)...
[2024-01-15 10:30:05] [Success] VPN connected successfully!
[2024-01-15 10:30:05] [Info] Waiting for 10 minutes before next action: Disconnect VPN
[10:30:15] Current State: Connected | Next Action: Disconnect VPN in 9m 50s
```

## 🔒 Security Best Practices

1. ✅ **Save credentials** in Windows VPN settings instead of passing as parameters
2. ✅ **Protect log files** - they may contain sensitive information
3. ✅ **Secure the script** - store in a protected directory
4. ⚠️ **Understand impact** - toggling VPN affects your network security

## ❓ Troubleshooting

| Issue | Solution |
|-------|----------|
| VPN not found | Run `Get-VpnConnection` to find exact name |
| Execution policy error | Run `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` |
| Access denied | Run PowerShell as Administrator |
| Connection fails | Test manually: `rasdial "YourVPNName"` |

See [SETUP-AND-USAGE.md](SETUP-AND-USAGE.md) for detailed troubleshooting.

## 🎯 Use Cases

- **IP Rotation**: Periodically change your IP address
- **Connection Management**: Avoid VPN timeout/disconnection issues
- **Bandwidth Management**: Cycle VPN to manage data usage
- **Testing**: Automated testing of VPN-dependent applications
- **Privacy**: Regular IP changes for enhanced privacy

## 📝 License

This project is provided as-is for personal and educational use.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

## ⚠️ Disclaimer

This script is provided for legitimate use cases only. Users are responsible for:
- Complying with their VPN provider's terms of service
- Understanding the security implications of toggling VPN
- Ensuring proper authorization for automated VPN management

## 🚀 CI/CD

This project uses **GitHub Actions** for automated building, testing, and releases:

- **Automated Builds:** Every push is built and tested
- **Automated Releases:** Push a version tag to create a release
- **Quality Checks:** Tests run on every PR

See [CI_CD_GUIDE.md](CI_CD_GUIDE.md) for details on the automated release process.

### For Developers

Create a new release:
```bash
git tag v1.3.0
git push origin v1.3.0
```

That's it! GitHub Actions will build, package, and publish the release automatically.

---

**Made with ❤️ for automated VPN management**