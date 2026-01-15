# WARP.md
#
# This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Common commands
- List available VPN connections (needed for `-VpnName`):
  - `Get-VpnConnection`
- Validate environment and prerequisites:
  - `.\Test-VpnSetup.ps1 -VpnName "YourVPNName"`
- Run the auto-toggle script (default 10‑minute cycles):
  - `.\VPN-AutoToggle.ps1 -VpnName "YourVPNName"`
- Quick test cycle (1 minute):
  - `.\VPN-AutoToggle.ps1 -VpnName "YourVPNName" -CycleDurationMinutes 1`
- Provide explicit credentials (only if not saved in Windows VPN settings):
  - `.\VPN-AutoToggle.ps1 -VpnName "YourVPNName" -Username "user@domain.com" -Password "YourPassword"`
- Tail the log:
  - `Get-Content .\VPN-AutoToggle.log -Wait`

## High-level architecture
- Two primary entry points:
  - `VPN-AutoToggle.ps1` is the main loop that alternates VPN connect/disconnect.
  - `Test-VpnSetup.ps1` is a prerequisite checker that validates PowerShell version, execution policy, VPN cmdlets, rasdial availability, and optional specific VPN connection.
- `VPN-AutoToggle.ps1` flow:
  - Defines helper functions for logging (`Write-Log`), connection status (`Test-VpnConnection`), connect/disconnect with retries (`Connect-VpnWithRetry`, `Disconnect-VpnWithRetry`), countdown waiting (`Wait-WithCountdown`), and graceful shutdown (`Stop-Script`).
  - Uses `rasdial` for connect/disconnect and `Get-VpnConnection` for status verification.
  - Maintains global state (`$script:CurrentState`, `$script:ShouldStop`) and runs an infinite cycle that alternates between connected and disconnected states, sleeping for `CycleDurationMinutes` between actions.
  - Registers Ctrl+C handlers to stop cleanly and logs final state on exit.
- `Test-VpnSetup.ps1`:
  - Runs an 8‑check sequence and summarizes pass/fail counts with recommendations.
  - When a `-VpnName` is provided, it verifies that specific connection and prints a recommended start command.

## Important docs to reference
- `readme.md` for overview and parameters
- `QUICK-START.md` for setup steps
- `SETUP-AND-USAGE.md` for detailed setup/troubleshooting
