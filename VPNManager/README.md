# VPN Manager - Windows Application

A modern Windows Forms application for automated VPN connection management with real-time status monitoring of VPN and MEGAsync.

## Features

### Core Functionality
- ✅ **Modern Windows UI** - Clean, user-friendly interface
- ✅ **VPN Auto-Toggle** - Automatically cycle VPN connections on/off
- ✅ **Real-Time Status Monitoring** - Live status updates for VPN and MEGAsync
- ✅ **Auto-Refresh** - Configurable refresh intervals (default: 5 seconds)
- ✅ **System Tray Support** - Minimize to system tray with context menu
- ✅ **Persistent Settings** - Configuration saved automatically
- ✅ **Single Instance** - Prevents multiple instances from running

### Status Monitoring
- **VPN Status**:
  - Connection state (Connected/Disconnected)
  - Visual status indicator (Green/Red)
  - Connection name display
  - Last update timestamp

- **MEGAsync Status**:
  - Process running detection
  - Sync status detection (Syncing/Idle)
  - Process ID display
  - Visual status indicator

## Requirements

- **Windows 10/11** or Windows Server 2016+
- **.NET 6.0 Runtime** (or SDK for building)
- **PowerShell 5.1+**
- **Configured VPN connection** in Windows
- **VPN-AutoToggle.ps1** script (must be in same directory as executable)

## Installation

### Option 1: Build from Source

1. **Install .NET 6.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/6.0
   - Or use Visual Studio 2022

2. **Build the Application**
   ```batch
   cd VPNManager
   build.bat
   ```

3. **Run the Application**
   ```batch
   cd bin\Release\net6.0-windows\
   VPNManager.exe
   ```

### Option 2: Download Pre-Built Executable

If you have a pre-built `VPNManager.exe`:
1. Place `VPNManager.exe` and `VPN-AutoToggle.ps1` in the same folder
2. Double-click `VPNManager.exe` to run

## Configuration

### First-Time Setup

1. **Open Settings**
   - Click the "Settings" button in the main window

2. **Configure VPN Settings**
   - Select your VPN connection from the dropdown
   - Click "↻" to refresh the VPN list if needed
   - Choose to use saved credentials (recommended) or provide username/password
   - Set cycle duration (default: 10 minutes)
   - Set max retries (default: 3)

3. **Configure General Settings**
   - Enable/disable system tray minimization
   - Enable/disable start minimized
   - Set status refresh interval (1-60 seconds)

4. **Click Apply or OK** to save settings

### Settings Location

Settings are stored in:
```
%AppData%\VPNManager\settings.json
```

## Usage

### Starting VPN Auto-Cycle

1. Configure your VPN settings (if not already done)
2. Click "Start VPN Cycle" button
3. The application will:
   - Connect to your VPN
   - Wait for the configured cycle duration
   - Disconnect from VPN
   - Wait for the configured cycle duration
   - Repeat the cycle

### Stopping VPN Auto-Cycle

- Click "Stop VPN Cycle" button
- The application will disconnect any active VPN connection

### Monitoring Status

- Status updates automatically every 5 seconds (configurable)
- Click "Refresh Now" for immediate status update
- Status indicators:
  - 🟢 Green = Connected/Running
  - 🔴 Red = Disconnected/Not Running
  - 🔵 Blue = Syncing (MEGAsync)

### System Tray

When minimized to system tray:
- Double-click tray icon to restore window
- Right-click for context menu:
  - Show
  - Start VPN
  - Stop VPN
  - Exit

## Project Structure

```
VPNManager/
├── VPNManager.csproj          # Project file
├── Program.cs                 # Application entry point
├── Models/
│   ├── AppSettings.cs        # Settings model with persistence
│   ├── VpnStatus.cs          # VPN status model
│   └── MegaStatus.cs         # MEGAsync status model
├── Services/
│   ├── VpnService.cs         # VPN management service
│   └── MegaService.cs        # MEGAsync monitoring service
├── Forms/
│   ├── MainForm.cs           # Main application window
│   └── SettingsForm.cs       # Settings dialog
├── build.bat                  # Build script
└── README.md                  # This file
```

## Building

### Using Build Script

```batch
cd VPNManager
build.bat
```

### Using .NET CLI

```batch
cd VPNManager
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true
```

### Using Visual Studio

1. Open `VPNManager.csproj`
2. Right-click project → Build
3. Or press `Ctrl+Shift+B`

## Troubleshooting

### VPN Not Found
- Ensure VPN is configured in Windows VPN settings
- Run `Get-VpnConnection` in PowerShell to list available VPNs
- Refresh VPN list in settings

### Application Won't Start
- Ensure .NET 6.0 Runtime is installed
- Check that `VPN-AutoToggle.ps1` is in the same directory
- Run as Administrator if VPN requires elevated privileges

### Status Not Updating
- Check refresh interval in settings
- Click "Refresh Now" button
- Restart the application

### MEGAsync Not Detected
- Ensure MEGAsync is installed and running
- Check that process name is "MEGAsync"
- Verify MEGAsync is installed in standard location

## Security Considerations

⚠️ **Important**:
- Credentials are stored in plain text in `%AppData%\VPNManager\settings.json`
- Use Windows saved credentials (recommended) instead of storing in app
- Protect the settings.json file with appropriate NTFS permissions
- Understand that toggling VPN affects your network security

## Development

### Adding New Features

1. **Models** (`Models/`): Add data models
2. **Services** (`Services/`): Add business logic
3. **Forms** (`Forms/`): Add UI components
4. **Update Program.cs** if new forms need to be launched

### Code Conventions

- Follow C# naming conventions
- Use async/await for I/O operations
- Implement proper error handling with try-catch
- Add XML documentation for public APIs
- Dispose of resources properly (IDisposable)

## License

This project is provided as-is for personal and educational use.

## Contributing

Contributions, issues, and feature requests are welcome!

## Disclaimer

This application is provided for legitimate use cases only. Users are responsible for:
- Complying with their VPN provider's terms of service
- Understanding the security implications of toggling VPN
- Ensuring proper authorization for automated VPN management
- Windows VPN connection requirements and limitations

---

**Built with ❤️ using .NET 6.0 and Windows Forms**
