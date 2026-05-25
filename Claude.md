# Claude.md

This file provides guidance for AI assistants (like Claude, WARP, etc.) when working with code in this repository.

## Project Overview

This is a VPN Auto-Toggle PowerShell project that automatically cycles VPN connections on and off at regular intervals. It's designed for Windows systems using built-in VPN management tools.

## Key Common Commands

### List VPN Connections
```powershell
Get-VpnConnection
```

### Validate Environment and Prerequisites
```powershell
.\Test-VpnSetup.ps1 -VpnName "YourVPNName"
```

### Run the Auto-Toggle Script (default 10-minute cycles)
```powershell
.\VPN-AutoToggle.ps1 -VpnName "YourVPNName"
```

### Quick Test Cycle (1 minute)
```powershell
.\VPN-AutoToggle.ps1 -VpnName "YourVPNName" -CycleDurationMinutes 1
```

### Provide Explicit Credentials (only if not saved in Windows VPN settings)
```powershell
.\VPN-AutoToggle.ps1 -VpnName "YourVPNName" -Username "user@domain.com" -Password "YourPassword"
```

### Monitor the Log File
```powershell
Get-Content .\VPN-AutoToggle.log -Wait
```

## High-Level Architecture

### Two Primary Entry Points

1. **VPN-AutoToggle.ps1** - Main automation script
   - Alternates VPN connect/disconnect cycles
   - Manages state, logging, and graceful shutdown
   - Handles retries and error recovery

2. **Test-VpnSetup.ps1** - Prerequisite checker
   - Validates PowerShell version, execution policy
   - Checks VPN cmdlets and rasdial availability
   - Verifies specific VPN connection exists
   - Provides 8-check sequence with pass/fail summary

### VPN-AutoToggle.ps1 Flow

- **Helper Functions**:
  - `Write-Log` - Centralized logging with timestamps and severity levels
  - `Test-VpnConnection` - Check current VPN connection status
  - `Connect-VpnWithRetry` - Connect with configurable retry logic
  - `Disconnect-VpnWithRetry` - Disconnect with configurable retry logic
  - `Wait-WithCountdown` - Display live countdown during wait periods
  - `Stop-Script` - Graceful shutdown handler

- **Technology Stack**:
  - Uses `rasdial` for connect/disconnect operations
  - Uses `Get-VpnConnection` for status verification
  - Maintains global state: `$script:CurrentState`, `$script:ShouldStop`

- **Main Loop**:
  - Infinite cycle alternating between connected/disconnected states
  - Sleeps for `CycleDurationMinutes` between state changes
  - Registers Ctrl+C handlers for clean exit
  - Logs all state transitions and errors

### Test-VpnSetup.ps1

- Runs comprehensive 8-check validation sequence
- Summarizes pass/fail counts with actionable recommendations
- When `-VpnName` is provided, verifies that specific connection
- Prints recommended start command based on validation results

## Important Documentation Files

- **[readme.md](readme.md)** - Project overview, features, and parameters
- **[QUICK-START.md](QUICK-START.md)** - 3-step setup guide
- **[SETUP-AND-USAGE.md](SETUP-AND-USAGE.md)** - Detailed setup and troubleshooting

## Script Parameters

### VPN-AutoToggle.ps1

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `VpnName` | ✅ Yes | - | Name of VPN connection (must match Windows VPN settings) |
| `Username` | ❌ No | - | VPN username (only if not saved in Windows) |
| `Password` | ❌ No | - | VPN password (only if not saved in Windows) |
| `CycleDurationMinutes` | ❌ No | 10 | Minutes to maintain each state (connected/disconnected) |
| `LogPath` | ❌ No | `.\VPN-AutoToggle.log` | Custom log file location |
| `MaxRetries` | ❌ No | 3 | Number of retry attempts for failed connections |

### Test-VpnSetup.ps1

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `VpnName` | ❌ No | - | Specific VPN connection to validate |

## Code Conventions

- **PowerShell Version**: 5.1+ required
- **Error Handling**: Comprehensive try-catch blocks with logging
- **Logging**: All operations logged with timestamps and severity levels (Info, Success, Warning, Error)
- **State Management**: Global script variables for tracking state
- **Security**: Credentials best handled through Windows VPN settings, not command-line parameters
- **Graceful Shutdown**: Ctrl+C handlers ensure clean exit with final state logging

## Security Considerations

⚠️ **Important Notes**:
- This script manages VPN connections and affects network security
- Log files may contain sensitive information - protect appropriately
- Credentials passed via command-line may be visible in process lists
- Best practice: Save credentials in Windows VPN settings instead of parameters
- Users must comply with VPN provider terms of service
- Understand the security implications of automatic VPN toggling

## Use Cases

- **IP Rotation**: Periodic IP address changes for privacy
- **Connection Management**: Prevent VPN timeout issues
- **Bandwidth Management**: Control VPN data usage
- **Testing**: Automated testing of VPN-dependent applications
- **Privacy**: Regular IP changes for enhanced privacy

## Development Guidelines

When modifying or extending this project:

1. Maintain comprehensive logging for all operations
2. Preserve graceful shutdown functionality
3. Ensure backward compatibility with existing parameters
4. Test thoroughly with `Test-VpnSetup.ps1`
5. Document new parameters in [readme.md](readme.md)
6. Update all relevant documentation files
7. Consider security implications of any changes
8. Test with both saved credentials and explicit credentials

## Testing Approach

Always validate changes using the test script:
```powershell
.\Test-VpnSetup.ps1 -VpnName "TestVPN"
```

This ensures:
- PowerShell environment is correct
- Required cmdlets are available
- VPN connection exists and is accessible
- rasdial command is functional
- Script files are present and valid

## Health Stack

- typecheck: dotnet build VPNManager/VPNManager.csproj
- lint: dotnet format VPNManager/VPNManager.csproj --verify-no-changes
- test: pwsh -Command "& ./Test-VpnSetup.ps1 -SkipAdminCheck -SkipVpnCheck; & ./Test-E2E-Integration.ps1"
- ps-lint: Invoke-ScriptAnalyzer -Path . -Recurse -IncludeDefaultRules
- deadcode: skipped (no tool detected)
