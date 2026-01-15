# VPN Auto-Toggle - Usage Examples

This document provides real-world examples and scenarios for using the VPN Auto-Toggle script.

## 📋 Table of Contents

1. [Basic Examples](#basic-examples)
2. [Advanced Scenarios](#advanced-scenarios)
3. [Automation & Scheduling](#automation--scheduling)
4. [Monitoring & Debugging](#monitoring--debugging)
5. [Common Workflows](#common-workflows)

---

## Basic Examples

### Example 1: Simple Start with Saved Credentials

```powershell
# Most common usage - VPN with saved credentials, 10-minute cycles
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"
```

**When to use**: Your VPN credentials are saved in Windows, and you want the default 10-minute cycle.

---

### Example 2: Quick Testing

```powershell
# Test with 1-minute cycles to verify everything works
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1
```

**When to use**: First-time setup or testing changes.

---

### Example 3: With Explicit Credentials

```powershell
# Provide username and password directly
.\VPN-AutoToggle.ps1 -VpnName "WorkVPN" -Username "john.doe@company.com" -Password "SecurePass123"
```

**When to use**: VPN doesn't have saved credentials or you need to use different credentials.

---

### Example 4: Custom Cycle Duration

```powershell
# 5-minute cycles for more frequent IP rotation
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 5
```

**When to use**: You need more frequent IP changes or shorter connection periods.

---

### Example 5: Custom Log Location

```powershell
# Save logs to a specific location
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -LogPath "C:\Logs\VPN\toggle-$(Get-Date -Format 'yyyy-MM-dd').log"
```

**When to use**: You want organized logs or need logs in a specific directory.

---

## Advanced Scenarios

### Scenario 1: High-Reliability Setup

```powershell
# Maximum retries for unstable connections
.\VPN-AutoToggle.ps1 `
    -VpnName "UnstableVPN" `
    -CycleDurationMinutes 10 `
    -MaxRetries 5 `
    -LogPath "C:\Logs\VPN\reliable.log"
```

**Use case**: VPN connection is unreliable and needs multiple retry attempts.

---

### Scenario 2: Rapid IP Rotation

```powershell
# Very short cycles for frequent IP changes
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 2
```

**Use case**: Web scraping, testing geo-restrictions, or applications requiring frequent IP changes.

---

### Scenario 3: Long-Duration Cycles

```powershell
# 30-minute cycles for less frequent toggling
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 30
```

**Use case**: Avoiding VPN session timeouts while minimizing connection churn.

---

### Scenario 4: Multiple VPN Monitoring

```powershell
# Terminal 1: Run VPN toggle
.\VPN-AutoToggle.ps1 -VpnName "VPN1" -LogPath ".\vpn1.log"

# Terminal 2: Monitor in real-time
Get-Content .\vpn1.log -Wait

# Terminal 3: Watch VPN status
while ($true) {
    Clear-Host
    Get-Date
    Get-VpnConnection | Format-Table Name, ConnectionStatus, ServerAddress
    Start-Sleep 5
}
```

**Use case**: Detailed monitoring and debugging of VPN behavior.

---

## Automation & Scheduling

### Example 1: Run at Startup (Task Scheduler)

**Create a scheduled task:**

1. Open Task Scheduler
2. Create Basic Task → Name: "VPN Auto-Toggle"
3. Trigger: "When the computer starts"
4. Action: "Start a program"
   - Program: `powershell.exe`
   - Arguments: 
     ```
     -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "C:\Scripts\VPN-AutoToggle.ps1" -VpnName "MyVPN" -LogPath "C:\Logs\vpn-toggle.log"
     ```
5. Finish and test

---

### Example 2: Run Minimized in Background

```powershell
# Start the script in a minimized window
Start-Process powershell -ArgumentList `
    "-NoProfile -ExecutionPolicy Bypass -File `"$PWD\VPN-AutoToggle.ps1`" -VpnName `"MyVPN`"" `
    -WindowStyle Minimized
```

**Use case**: Run in background without cluttering your screen.

---

### Example 3: Run During Specific Hours

Create a wrapper script `Run-VpnDuringBusinessHours.ps1`:

```powershell
# Only run between 9 AM and 5 PM
$currentHour = (Get-Date).Hour

if ($currentHour -ge 9 -and $currentHour -lt 17) {
    .\VPN-AutoToggle.ps1 -VpnName "WorkVPN" -CycleDurationMinutes 10
}
else {
    Write-Host "Outside business hours. VPN toggle not started."
}
```

---

### Example 4: Auto-Restart on Failure

Create a wrapper script `Run-VpnWithAutoRestart.ps1`:

```powershell
while ($true) {
    try {
        .\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 10
    }
    catch {
        Write-Host "Script crashed. Restarting in 30 seconds..."
        Start-Sleep -Seconds 30
    }
}
```

---

## Monitoring & Debugging

### Example 1: Real-Time Log Monitoring

```powershell
# In a separate PowerShell window
Get-Content .\VPN-AutoToggle.log -Wait -Tail 20
```

---

### Example 2: Check VPN Status Continuously

```powershell
# Monitor VPN status every 5 seconds
while ($true) {
    Clear-Host
    Write-Host "=== VPN Status Monitor ===" -ForegroundColor Cyan
    Write-Host "Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n"
    
    Get-VpnConnection | Select-Object Name, ConnectionStatus, ServerAddress | Format-Table
    
    Start-Sleep -Seconds 5
}
```

---

### Example 3: Log Analysis

```powershell
# Count successful connections
(Get-Content .\VPN-AutoToggle.log | Select-String "connected successfully").Count

# Count failed attempts
(Get-Content .\VPN-AutoToggle.log | Select-String "Failed to connect").Count

# Show only errors
Get-Content .\VPN-AutoToggle.log | Select-String "\[Error\]"

# Show last 10 entries
Get-Content .\VPN-AutoToggle.log -Tail 10
```

---

## Common Workflows

### Workflow 1: First-Time Setup and Testing

```powershell
# Step 1: Verify setup
.\Test-VpnSetup.ps1 -VpnName "MyVPN"

# Step 2: Quick test (1-minute cycles)
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1

# Step 3: Monitor for 5 minutes, then stop with Ctrl+C

# Step 4: Check logs
Get-Content .\VPN-AutoToggle.log

# Step 5: If successful, run with normal timing
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"
```

---

### Workflow 2: Production Deployment

```powershell
# Step 1: Create dedicated directory
New-Item -Path "C:\Scripts\VPN-AutoToggle" -ItemType Directory -Force
Copy-Item .\VPN-AutoToggle.ps1 -Destination "C:\Scripts\VPN-AutoToggle\"

# Step 2: Create log directory
New-Item -Path "C:\Logs\VPN" -ItemType Directory -Force

# Step 3: Test from new location
cd C:\Scripts\VPN-AutoToggle
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1 -LogPath "C:\Logs\VPN\toggle.log"

# Step 4: Create scheduled task (see Automation examples above)

# Step 5: Monitor logs
Get-Content C:\Logs\VPN\toggle.log -Wait
```

---

### Workflow 3: Troubleshooting Connection Issues

```powershell
# Step 1: Test manual connection
rasdial "MyVPN"
Start-Sleep 5
Get-VpnConnection -Name "MyVPN"
rasdial "MyVPN" /disconnect

# Step 2: Run with verbose logging and short cycles
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1 -MaxRetries 5

# Step 3: Monitor in separate window
Get-Content .\VPN-AutoToggle.log -Wait

# Step 4: Check for patterns in failures
Get-Content .\VPN-AutoToggle.log | Select-String "Error|Failed"
```

---

## Tips & Best Practices

### 💡 Tip 1: Use Saved Credentials
Save your VPN credentials in Windows to avoid passing passwords as parameters:
```powershell
# Better (secure)
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"

# Avoid (password in command history)
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -Password "MyPassword"
```

### 💡 Tip 2: Start with Short Cycles for Testing
Always test with 1-2 minute cycles first:
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1
```

### 💡 Tip 3: Use Descriptive Log Names
Include dates in log file names for better organization:
```powershell
$logFile = "vpn-toggle-$(Get-Date -Format 'yyyy-MM-dd').log"
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -LogPath $logFile
```

### 💡 Tip 4: Monitor First Run
Always monitor the first few cycles to ensure everything works correctly.

### 💡 Tip 5: Graceful Shutdown
Always use Ctrl+C to stop the script - never force-close the window.

---

For more information, see:
- [QUICK-START.md](QUICK-START.md) - Quick reference
- [SETUP-AND-USAGE.md](SETUP-AND-USAGE.md) - Detailed documentation
- [readme.md](readme.md) - Project overview

