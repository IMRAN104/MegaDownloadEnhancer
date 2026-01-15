# VPN Auto-Toggle Script - Setup and Usage Guide

## Overview
This PowerShell script automatically toggles your VPN connection on and off at regular intervals (default: 10 minutes). It includes robust error handling, logging, and graceful shutdown capabilities.

## Prerequisites

### System Requirements
- **Operating System**: Windows 10/11 or Windows Server 2016+
- **PowerShell**: Version 5.1 or higher (check with `$PSVersionTable.PSVersion`)
- **VPN Connection**: A configured VPN connection in Windows Network Connections
- **Permissions**: Administrator privileges recommended for VPN operations

### Verify PowerShell Version
```powershell
$PSVersionTable.PSVersion
```

## Initial Setup

### Step 1: Configure Your VPN Connection

1. **Create a VPN connection** in Windows:
   - Go to Settings → Network & Internet → VPN
   - Click "Add a VPN connection"
   - Fill in your VPN details and give it a memorable name (e.g., "MyVPN")
   - Save credentials if you want to run without username/password parameters

2. **Test manual connection**:
   ```powershell
   # List all VPN connections
   Get-VpnConnection
   
   # Test connecting manually
   rasdial "MyVPN"
   
   # Test disconnecting manually
   rasdial "MyVPN" /disconnect
   ```

### Step 2: Enable Script Execution

PowerShell scripts require execution policy to be set. Run PowerShell as Administrator:

```powershell
# Check current execution policy
Get-ExecutionPolicy

# Set execution policy (choose one):
# Option 1: Allow local scripts (recommended)
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser

# Option 2: Allow all scripts (less secure)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
```

### Step 3: Download and Prepare the Script

1. Save `VPN-AutoToggle.ps1` to a folder (e.g., `C:\Scripts\VPN-AutoToggle\`)
2. Open PowerShell and navigate to the folder:
   ```powershell
   cd C:\Scripts\VPN-AutoToggle
   ```

## Usage

### Basic Usage (Saved Credentials)

If your VPN connection has saved credentials:

```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN"
```

### Advanced Usage (With Credentials)

If you need to provide credentials:

```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -Username "user@domain.com" -Password "YourPassword"
```

### Custom Cycle Duration

Change the toggle interval (e.g., 5 minutes instead of 10):

```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 5
```

### Custom Log Location

Specify a custom log file path:

```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -LogPath "C:\Logs\VPN-Toggle.log"
```

### All Parameters

```powershell
.\VPN-AutoToggle.ps1 `
    -VpnName "MyVPN" `
    -Username "user@domain.com" `
    -Password "SecurePass123" `
    -CycleDurationMinutes 10 `
    -LogPath "C:\Logs\VPN-Toggle.log" `
    -MaxRetries 3
```

## Testing Approach

### Safe Testing Steps

#### Test 1: Verify VPN Connection Exists

```powershell
# List all VPN connections
Get-VpnConnection

# Check specific VPN
Get-VpnConnection -Name "MyVPN"
```

#### Test 2: Dry Run with Short Duration

Test with a 1-minute cycle to verify functionality quickly:

```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 1
```

**What to observe:**
- Script should connect to VPN
- Wait 1 minute with countdown
- Disconnect from VPN
- Wait 1 minute with countdown
- Repeat

#### Test 3: Monitor Script Behavior

Open two PowerShell windows:

**Window 1** - Run the script:
```powershell
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 2
```

**Window 2** - Monitor VPN status:
```powershell
# Run this in a loop to watch status changes
while ($true) {
    Clear-Host
    Get-Date
    Get-VpnConnection -Name "MyVPN" | Select-Object Name, ConnectionStatus, ServerAddress
    Start-Sleep -Seconds 5
}
```

#### Test 4: Verify Logging

After running the script, check the log file:

```powershell
# View the log file
Get-Content .\VPN-AutoToggle.log -Tail 50

# Monitor log in real-time
Get-Content .\VPN-AutoToggle.log -Wait
```

#### Test 5: Test Graceful Shutdown

1. Start the script
2. Press `Ctrl+C` during any phase (connecting, waiting, disconnecting)
3. Verify the script stops gracefully with a shutdown message

### Verification Checklist

- [ ] Script starts without errors
- [ ] VPN connects successfully
- [ ] Countdown timer displays correctly
- [ ] VPN disconnects successfully
- [ ] Cycle repeats automatically
- [ ] Log file is created and updated
- [ ] Ctrl+C stops the script gracefully
- [ ] Error handling works (test by using wrong VPN name)

## Stopping the Script

### Method 1: Graceful Shutdown (Recommended)
Press `Ctrl+C` in the PowerShell window. The script will:
- Detect the interrupt signal
- Complete current operation safely
- Log the shutdown
- Exit cleanly

### Method 2: Close Window
Simply close the PowerShell window. Note: This may leave VPN in current state.

### Method 3: Task Manager
If the script becomes unresponsive:
1. Open Task Manager (Ctrl+Shift+Esc)
2. Find "Windows PowerShell" process
3. End the process

## Troubleshooting

### Issue: "VPN connection not found"
**Solution**: 
```powershell
# List all VPN connections to find the correct name
Get-VpnConnection
# Use the exact name shown in the output
```

### Issue: "Access Denied" or permission errors
**Solution**: Run PowerShell as Administrator
```powershell
# Right-click PowerShell → Run as Administrator
```

### Issue: Script won't run - "execution policy" error
**Solution**: 
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue: VPN connects but script reports failure
**Solution**: Increase wait time in the script or check VPN connection stability

### Issue: Credentials not working
**Solution**: 
- Verify credentials are correct
- Try saving credentials in Windows VPN settings
- Run without -Username and -Password parameters

## Running as Background Service

### Option 1: Run in Minimized Window
```powershell
Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"C:\Scripts\VPN-AutoToggle\VPN-AutoToggle.ps1`" -VpnName `"MyVPN`"" -WindowStyle Minimized
```

### Option 2: Schedule with Task Scheduler
1. Open Task Scheduler
2. Create Basic Task
3. Set trigger (e.g., "At startup")
4. Action: Start a program
   - Program: `powershell.exe`
   - Arguments: `-NoProfile -ExecutionPolicy Bypass -File "C:\Scripts\VPN-AutoToggle\VPN-AutoToggle.ps1" -VpnName "MyVPN"`
5. Configure to run whether user is logged in or not

## Security Considerations

⚠️ **Important Security Notes:**

1. **Avoid hardcoding passwords**: Use saved credentials in Windows VPN settings instead of passing passwords as parameters
2. **Protect log files**: Log files may contain sensitive information
3. **Script permissions**: Keep the script in a secure location with appropriate file permissions
4. **Network security**: Understand that toggling VPN affects your network security posture

## Advanced Configuration

### Modify Retry Logic
Edit the script and change `$MaxRetries` default value (line 52)

### Change Wait Intervals
Edit retry wait times in `Connect-VpnWithRetry` and `Disconnect-VpnWithRetry` functions

### Customize Logging
Modify the `Write-Log` function to change log format or add additional logging destinations

## Support and Feedback

For issues or questions:
1. Check the log file for detailed error messages
2. Review this documentation
3. Test with shorter cycle times for faster debugging
4. Verify VPN connection works manually before using the script

