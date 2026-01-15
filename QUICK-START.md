# Quick Start Guide - VPN Auto-Toggle

## 🚀 Get Started in 3 Steps

### Step 1: Find Your VPN Name
```powershell
Get-VpnConnection
```
Copy the exact name of your VPN connection.

### Step 2: Enable Script Execution (One-time setup)
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Step 3: Run the Script
```powershell
# Replace "MyVPN" with your actual VPN name
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"
```

## 🧪 Quick Test (1-minute cycles)
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1
```

## 🛑 Stop the Script
Press `Ctrl+C` at any time for graceful shutdown.

## 📋 What You'll See

```
========================================
   VPN Auto-Toggle Script v1.0
========================================

[2024-01-15 10:30:00] [Info] Script started with parameters:
[2024-01-15 10:30:00] [Info]   VPN Name: MyVPN
[2024-01-15 10:30:00] [Info]   Cycle Duration: 10 minutes
[2024-01-15 10:30:05] [Success] VPN connected successfully!
[2024-01-15 10:30:05] [Info] Waiting for 10 minutes before next action: Disconnect VPN
[10:30:15] Current State: Connected | Next Action: Disconnect VPN in 9m 50s
```

## 📝 Common Commands

### With Saved Credentials
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"
```

### With Username/Password
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -Username "user@domain.com" -Password "YourPassword"
```

### Custom Duration (5 minutes)
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 5
```

### Custom Log Location
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -LogPath "C:\Logs\vpn.log"
```

## ❓ Troubleshooting

| Problem | Solution |
|---------|----------|
| "VPN connection not found" | Run `Get-VpnConnection` to find exact name |
| "Execution policy" error | Run `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` |
| "Access denied" | Run PowerShell as Administrator |
| Script won't connect | Verify VPN works manually: `rasdial "MyVPN"` |

## 📖 Need More Help?
See [SETUP-AND-USAGE.md](SETUP-AND-USAGE.md) for detailed documentation.

## ⚙️ Parameters Reference

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-VpnName` | ✅ Yes | - | Name of VPN connection |
| `-Username` | ❌ No | - | VPN username (if not saved) |
| `-Password` | ❌ No | - | VPN password (if not saved) |
| `-CycleDurationMinutes` | ❌ No | 10 | Minutes for each state |
| `-LogPath` | ❌ No | `.\VPN-AutoToggle.log` | Log file location |
| `-MaxRetries` | ❌ No | 3 | Connection retry attempts |

## 🔒 Security Tips

1. ✅ **DO**: Save credentials in Windows VPN settings
2. ✅ **DO**: Use `-VpnName "MyVPN"` without password parameters
3. ❌ **DON'T**: Hardcode passwords in scripts or shortcuts
4. ❌ **DON'T**: Share log files (may contain sensitive info)

## 🎯 Example Scenarios

### Scenario 1: Basic Home Use
```powershell
# 10-minute cycles with saved credentials
.\VPN-AutoToggle.ps1 -VpnName "HomeVPN"
```

### Scenario 2: Quick Testing
```powershell
# 1-minute cycles for testing
.\VPN-AutoToggle.ps1 -VpnName "TestVPN" -CycleDurationMinutes 1
```

### Scenario 3: Work VPN with Credentials
```powershell
# 15-minute cycles with explicit credentials
.\VPN-AutoToggle.ps1 -VpnName "WorkVPN" -Username "john.doe@company.com" -Password "SecurePass123" -CycleDurationMinutes 15
```

### Scenario 4: Background Service
```powershell
# Run minimized in background
Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PWD\VPN-AutoToggle.ps1`" -VpnName `"MyVPN`"" -WindowStyle Minimized
```

## 📊 Monitoring

### View Live Log
```powershell
Get-Content .\VPN-AutoToggle.log -Wait
```

### Check VPN Status
```powershell
Get-VpnConnection -Name "MyVPN" | Select-Object Name, ConnectionStatus
```

### Watch Status in Real-Time
```powershell
while ($true) { 
    Clear-Host
    Get-VpnConnection -Name "MyVPN" | Format-Table Name, ConnectionStatus, ServerAddress
    Start-Sleep 5 
}
```

