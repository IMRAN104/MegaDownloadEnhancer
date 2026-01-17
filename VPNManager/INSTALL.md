# VPN Manager - Installation & Quick Start Guide

## Quick Installation

### Step 1: Install .NET 6.0 Runtime

**Download and install .NET 6.0 Runtime:**
- Windows x64: https://dotnet.microsoft.com/download/dotnet/6.0
- Select "Runtime" (not SDK) for running pre-built applications
- Or select "SDK" if you want to build from source

### Step 2: Build the Application

**Option A: Using the build script (Recommended)**
```batch
cd VPNManager
build.bat
```

**Option B: Using .NET CLI**
```batch
cd VPNManager
dotnet build -c Release
```

**Option C: Using Visual Studio**
1. Open `VPNManager.csproj`
2. Press `Ctrl+Shift+B` to build
3. Find output in `bin\Release\net6.0-windows\`

### Step 3: Prepare the Application

1. Navigate to the build output:
   ```batch
   cd bin\Release\net6.0-windows\
   ```

2. Ensure both files are present:
   - `VPNManager.exe`
   - `VPN-AutoToggle.ps1` (should be copied automatically)

3. If `VPN-AutoToggle.ps1` is missing:
   ```batch
   copy ..\..\..\VPN-AutoToggle.ps1 .
   ```

### Step 4: Run the Application

**Double-click `VPNManager.exe`**

Or from command line:
```batch
VPNManager.exe
```

## First-Time Configuration

### 1. Configure VPN Settings

1. Click **Settings** button
2. Go to **VPN Settings** tab
3. Click **↻** to refresh VPN list (if needed)
4. Select your VPN connection from dropdown
5. Choose credential method:
   - ✅ **Use saved credentials** (Recommended) - Leave username/password empty
   - ❌ **Manual credentials** - Enter username and password
6. Set cycle duration (default: 10 minutes)
7. Set max retries (default: 3)
8. Click **OK** or **Apply**

### 2. Start VPN Auto-Cycle

1. Click **Start VPN Cycle** button
2. Status will show "Connected" when VPN is active
3. Application will automatically toggle VPN on/off every 10 minutes

### 3. Monitor Status

- **VPN Status**: Shows connection state with colored indicator
- **MEGAsync Status**: Shows if MEGAsync is running/syncing
- Status auto-refreshes every 5 seconds (configurable)

## System Requirements

### Minimum Requirements
- Windows 10/11 or Windows Server 2016+
- .NET 6.0 Runtime or later
- PowerShell 5.1+
- Configured VPN connection in Windows
- 50 MB free disk space

### Recommended Requirements
- Windows 10/11 (64-bit)
- .NET 6.0 Runtime
- Administrator privileges (for VPN management)
- 100 MB free disk space

## Troubleshooting

### Build Errors

**Error: .NET SDK not installed**
```
Solution: Install .NET 6.0 SDK from https://dotnet.microsoft.com/download
```

**Error: No VPN connections found**
```
Solution:
1. Open Windows Settings → Network & Internet → VPN
2. Add a VPN connection if none exists
3. Run Get-VpnConnection in PowerShell to verify
```

### Runtime Errors

**Application won't start**
```
Solution:
1. Install .NET 6.0 Runtime (not just SDK)
2. Verify VPN-AutoToggle.ps1 is in same folder as exe
3. Run as Administrator
```

**VPN not found error**
```
Solution:
1. Open Settings and refresh VPN list
2. Verify VPN name matches Windows VPN settings exactly
3. Use Get-VpnConnection in PowerShell to check VPN name
```

**Credentials not working**
```
Solution:
1. Use saved credentials (recommended)
2. Or verify username/password are correct
3. Check VPN provider requirements
```

## Verification

### Verify Installation

1. **Check .NET Runtime:**
   ```batch
   dotnet --list-runtimes
   ```
   Should show: `Microsoft.NETCore.App 6.0.x`

2. **Check PowerShell:**
   ```batch
   $PSVersionTable.PSVersion
   ```
   Should be 5.1 or higher

3. **Check VPN:**
   ```batch
   Get-VpnConnection
   ```
   Should list your VPN connections

4. **Check Files:**
   ```batch
   dir VPNManager.exe
   dir VPN-AutoToggle.ps1
   ```

## Advanced Usage

### Run as Administrator

Create a shortcut and set to run as admin:
1. Right-click `VPNManager.exe`
2. Send to → Desktop (create shortcut)
3. Right-click shortcut → Properties
4. Shortcut tab → Advanced
5. Check "Run as administrator"
6. Click OK

### Start with Windows

1. Press `Win+R`, type `shell:startup`
2. Create shortcut to `VPNManager.exe` in startup folder
3. Enable "Start minimized" in application settings

### Portable Version

For a portable version (no installation):
1. Build application
2. Copy entire `bin\Release\net6.0-windows\` folder to USB drive
3. Run from USB drive on any Windows 10/11 PC with .NET 6.0 Runtime

## Uninstallation

1. Stop VPN cycle if running
2. Close application
3. Delete application folder
4. Delete settings: `%AppData%\VPNManager\`

## Support

For issues or questions:
1. Check this guide
2. Review main README.md
3. Check SETUP-AND-USAGE.md in parent directory
4. Review logs: `VPN-AutoToggle.log`

## Next Steps

- ✅ Configure your VPN settings
- ✅ Test with short cycle duration (1-2 minutes)
- ✅ Monitor status for a few cycles
- ✅ Adjust settings as needed
- ✅ Set to start with Windows (optional)

---

**Enjoy automated VPN management! 🚀**
