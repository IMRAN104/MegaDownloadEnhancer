<#
.SYNOPSIS
    Automated VPN Toggle Script - Cycles VPN connection on/off every 10 minutes

.DESCRIPTION
    This script automatically enables and disables a VPN connection in a continuous cycle.
    Each state (connected/disconnected) lasts for a configurable duration (default: 10 minutes).
    Includes logging, error handling, and graceful shutdown capabilities.

.PARAMETER VpnName
    The name of the VPN connection as shown in Windows Network Connections

.PARAMETER Username
    Username for VPN authentication (optional if credentials are saved)

.PARAMETER Password
    Password for VPN authentication (optional if credentials are saved)

.PARAMETER CycleDurationMinutes
    Duration in minutes for each state (default: 10)

.PARAMETER LogPath
    Path to the log file (default: .\VPN-AutoToggle.log)

.EXAMPLE
    .\VPN-AutoToggle.ps1 -VpnName "MyVPN"
    
.EXAMPLE
    .\VPN-AutoToggle.ps1 -VpnName "MyVPN" -Username "user@domain.com" -Password "SecurePass123" -CycleDurationMinutes 5

.NOTES
    Author: VPN Automation Script
    Version: 1.0
    Requires: PowerShell 5.1 or higher, Administrator privileges recommended
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false, HelpMessage="Enter the VPN connection name")]
    [string]$VpnName = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "",
    
    [Parameter(Mandatory=$false)]
    [int]$CycleDurationMinutes = 10,
    
    [Parameter(Mandatory=$false)]
    [string]$LogPath = ".\VPN-AutoToggle.log",

    [Parameter(Mandatory=$false)]
    [int]$MaxRetries = 3
)

# Global variables
$script:ShouldStop = $false
$script:CurrentState = "Disconnected"
# Early exit if VPN name not provided
if (-not $VpnName) {
    Write-Host "VPN name is required. Available VPN connections:" -ForegroundColor Yellow
    try {
        $connections = Get-VpnConnection -ErrorAction Stop
        if ($connections.Count -gt 0) {
            $connections | ForEach-Object { Write-Host "  - $($_.Name) [$($_.ConnectionStatus)]" -ForegroundColor Gray }
        }
        else {
            Write-Host "  (No VPN connections found)" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "  (Unable to list VPN connections: $($_.Exception.Message))" -ForegroundColor Red
    }
    return
}

#region Helper Functions

function Write-Log {
    <#
    .SYNOPSIS
        Writes a message to both console and log file
    #>
    param(
        [string]$Message,
        [ValidateSet('Info', 'Warning', 'Error', 'Success')]
        [string]$Level = 'Info'
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    # Color coding for console output
    $color = switch ($Level) {
        'Info'    { 'White' }
        'Warning' { 'Yellow' }
        'Error'   { 'Red' }
        'Success' { 'Green' }
    }
    
    Write-Host $logMessage -ForegroundColor $color
    Add-Content -Path $LogPath -Value $logMessage
}

function Test-VpnConnection {
    <#
    .SYNOPSIS
        Checks if the VPN connection exists and returns its current status
    #>
    param([string]$Name)
    
    try {
        $vpn = Get-VpnConnection -Name $Name -ErrorAction Stop
        return @{
            Exists = $true
            Connected = ($vpn.ConnectionStatus -eq 'Connected')
            Status = $vpn.ConnectionStatus
        }
    }
    catch {
        return @{
            Exists = $false
            Connected = $false
            Status = "Not Found"
        }
    }
}

function Connect-VpnWithRetry {
    <#
    .SYNOPSIS
        Attempts to connect to VPN with retry logic
    #>
    param(
        [string]$Name,
        [string]$User,
        [string]$Pass,
        [int]$Retries
    )
    
    for ($i = 1; $i -le $Retries; $i++) {
        try {
            Write-Log "Attempting to connect to VPN '$Name' (Attempt $i of $Retries)..." -Level Info
            
            if ($User -and $Pass) {
                # Use rasdial for username/password authentication
                $result = rasdial $Name $User $Pass 2>&1
            }
            else {
                # Use built-in PowerShell cmdlet (uses saved credentials)
                rasdial $Name 2>&1 | Out-Null
            }
            
            # Wait a moment for connection to establish
            Start-Sleep -Seconds 5
            
            # Verify connection
            $status = Test-VpnConnection -Name $Name
            if ($status.Connected) {
                Write-Log "VPN connected successfully!" -Level Success
                $script:CurrentState = "Connected"
                return $true
            }
            else {
                Write-Log "Connection attempt completed but VPN not connected. Status: $($status.Status)" -Level Warning
            }
        }
        catch {
            Write-Log "Connection attempt failed: $($_.Exception.Message)" -Level Error
        }
        
        if ($i -lt $Retries) {
            Write-Log "Waiting 10 seconds before retry..." -Level Info
            Start-Sleep -Seconds 10
        }
    }
    
    Write-Log "Failed to connect to VPN after $Retries attempts" -Level Error
    return $false
}

function Disconnect-VpnWithRetry {
    <#
    .SYNOPSIS
        Attempts to disconnect from VPN with retry logic
    #>
    param(
        [string]$Name,
        [int]$Retries
    )

    for ($i = 1; $i -le $Retries; $i++) {
        try {
            Write-Log "Attempting to disconnect from VPN '$Name' (Attempt $i of $Retries)..." -Level Info

            # Use rasdial to disconnect
            rasdial $Name /disconnect 2>&1 | Out-Null

            # Wait a moment for disconnection
            Start-Sleep -Seconds 3

            # Verify disconnection
            $status = Test-VpnConnection -Name $Name
            if (-not $status.Connected) {
                Write-Log "VPN disconnected successfully!" -Level Success
                $script:CurrentState = "Disconnected"
                return $true
            }
            else {
                Write-Log "Disconnection attempt completed but VPN still connected. Status: $($status.Status)" -Level Warning
            }
        }
        catch {
            Write-Log "Disconnection attempt failed: $($_.Exception.Message)" -Level Error
        }

        if ($i -lt $Retries) {
            Write-Log "Waiting 5 seconds before retry..." -Level Info
            Start-Sleep -Seconds 5
        }
    }

    Write-Log "Failed to disconnect from VPN after $Retries attempts" -Level Error
    return $false
}

function Wait-WithCountdown {
    <#
    .SYNOPSIS
        Waits for specified duration with countdown display and interrupt capability
    #>
    param(
        [int]$Minutes,
        [string]$NextAction
    )

    $totalSeconds = $Minutes * 60
    $endTime = (Get-Date).AddSeconds($totalSeconds)

    Write-Log "Waiting for $Minutes minutes before next action: $NextAction" -Level Info

    while ((Get-Date) -lt $endTime -and -not $script:ShouldStop) {
        $remaining = ($endTime - (Get-Date))
        $remainingMinutes = [math]::Floor($remaining.TotalMinutes)
        $remainingSeconds = $remaining.Seconds

        # Update progress every 30 seconds
        if ($remaining.TotalSeconds % 30 -lt 1) {
            Write-Host "`r[$(Get-Date -Format 'HH:mm:ss')] Current State: $script:CurrentState | Next Action: $NextAction in ${remainingMinutes}m ${remainingSeconds}s" -NoNewline -ForegroundColor Cyan
        }

        Start-Sleep -Seconds 1
    }

    Write-Host "" # New line after countdown

    if ($script:ShouldStop) {
        Write-Log "Wait interrupted by shutdown signal" -Level Warning
        return $false
    }

    return $true
}

function Stop-Script {
    <#
    .SYNOPSIS
        Handles graceful shutdown
    #>
    Write-Log "`n`nShutdown signal received. Initiating graceful shutdown..." -Level Warning
    $script:ShouldStop = $true
}

#endregion

#region Main Script

function Start-VpnAutoToggle {
    <#
    .SYNOPSIS
        Main function that runs the VPN toggle loop
    #>

    # Display startup banner
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "   VPN Auto-Toggle Script v1.0" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan

    Write-Log "Script started with parameters:" -Level Info
    Write-Log "  VPN Name: $VpnName" -Level Info
    Write-Log "  Cycle Duration: $CycleDurationMinutes minutes" -Level Info
    Write-Log "  Max Retries: $MaxRetries" -Level Info
    Write-Log "  Log Path: $LogPath" -Level Info
    Write-Log "  Username: $(if($Username){'Provided'}else{'Not provided (using saved credentials)'})" -Level Info

    # Verify VPN connection exists
    Write-Log "Verifying VPN connection exists..." -Level Info
    $vpnCheck = Test-VpnConnection -Name $VpnName

    if (-not $vpnCheck.Exists) {
        Write-Log "ERROR: VPN connection '$VpnName' not found!" -Level Error
        Write-Log "Available VPN connections:" -Level Info
        Get-VpnConnection | ForEach-Object { Write-Log "  - $($_.Name)" -Level Info }
        return
    }

    Write-Log "VPN connection found. Current status: $($vpnCheck.Status)" -Level Success

    # Set initial state based on current VPN status
    if ($vpnCheck.Connected) {
        Write-Log "VPN is currently connected. Will disconnect first." -Level Info
        $script:CurrentState = "Connected"
    }
    else {
        Write-Log "VPN is currently disconnected. Will connect first." -Level Info
        $script:CurrentState = "Disconnected"
    }

    Write-Log "`nStarting VPN auto-toggle cycle. Press Ctrl+C to stop gracefully.`n" -Level Info

    # Main loop
    $cycleCount = 0
    while (-not $script:ShouldStop) {
        $cycleCount++
        Write-Log "========== Cycle #$cycleCount ==========" -Level Info

        if ($script:CurrentState -eq "Disconnected") {
            # Connect to VPN
            $success = Connect-VpnWithRetry -Name $VpnName -User $Username -Pass $Password -Retries $MaxRetries

            if (-not $success) {
                Write-Log "Failed to connect to VPN. Waiting 1 minute before retry..." -Level Error
                Start-Sleep -Seconds 60
                continue
            }

            # Wait while connected
            if (-not (Wait-WithCountdown -Minutes $CycleDurationMinutes -NextAction "Disconnect VPN")) {
                break
            }
        }
        else {
            # Disconnect from VPN
            $success = Disconnect-VpnWithRetry -Name $VpnName -Retries $MaxRetries

            if (-not $success) {
                Write-Log "Failed to disconnect from VPN. Waiting 1 minute before retry..." -Level Error
                Start-Sleep -Seconds 60
                continue
            }

            # Wait while disconnected
            if (-not (Wait-WithCountdown -Minutes $CycleDurationMinutes -NextAction "Connect VPN")) {
                break
            }
        }
    }

    Write-Log "`nScript stopped after $cycleCount cycles." -Level Info
    Write-Log "Final VPN state: $script:CurrentState" -Level Info
    Write-Log "Shutdown complete." -Level Success
}

#endregion

# Register Ctrl+C handler
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { Stop-Script } | Out-Null
$null = [Console]::TreatControlCAsInput = $false

# Trap Ctrl+C
try {
    [Console]::CancelKeyPress.Add({
        param($sender, $e)
        $e.Cancel = $true
        Stop-Script
    })
}
catch {
    # Fallback for environments where Console.CancelKeyPress is not available
}

# Start the main script
try {
    Start-VpnAutoToggle
}
catch {
    Write-Log "Unexpected error occurred: $($_.Exception.Message)" -Level Error
    Write-Log "Stack trace: $($_.ScriptStackTrace)" -Level Error
}
finally {
    Write-Host "`nPress any key to exit..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

