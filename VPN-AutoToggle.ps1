<#
.SYNOPSIS
    Automated VPN & MEGAsync Toggle Script
.DESCRIPTION
    Automatically cycles VPN connection on/off at regular intervals with optional MEGAsync restart support.
.PARAMETER VpnName
    Name of the VPN connection (use "CloudflareWARP" for WARP, or any Windows VPN connection name)
.PARAMETER UseWarp
    Use Cloudflare WARP instead of Windows VPN (automatically enabled if VpnName is "CloudflareWARP")
.PARAMETER CycleDurationMinutes
    Duration in minutes for each connection state (default: 10)
.PARAMETER MaxRetries
    Maximum number of connection retry attempts (default: 3)
.PARAMETER MegasyncPath
    Path to MEGAsync.exe (default: auto-detect in common locations)
.PARAMETER MegasyncRestartDelaySeconds
    Delay in seconds before restarting MEGAsync (default: 5)
.PARAMETER WarpUiPath
    Path to Cloudflare WARP UI executable (default: standard installation path)
.PARAMETER LogPath
    Path to log file (default: .\VPN-AutoToggle.log)
.EXAMPLE
    .\VPN-AutoToggle.ps1 -VpnName "CloudflareWARP"
    Uses Cloudflare WARP with default 10-minute cycles
.EXAMPLE
    .\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 5
    Uses Windows VPN "MyVPN" with 5-minute cycles
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$VpnName = "CloudflareWARP",
    
    [Parameter(Mandatory = $false)]
    [bool]$UseWarp = $false,
    
    [Parameter(Mandatory = $false)]
    [int]$CycleDurationMinutes = 10,
    
    [Parameter(Mandatory = $false)]
    [int]$MaxRetries = 3,
    
    [Parameter(Mandatory = $false)]
    [string]$MegasyncPath = "",
    
    [Parameter(Mandatory = $false)]
    [int]$MegasyncRestartDelaySeconds = 5,
    
    [Parameter(Mandatory = $false)]
    [string]$WarpUiPath = "C:\Program Files\Cloudflare\Cloudflare WARP\Cloudflare WARP.exe",
    
    [Parameter(Mandatory = $false)]
    [string]$LogPath = ".\VPN-AutoToggle.log"
)

# Auto-detect WARP usage based on VPN name
if ($VpnName -eq "CloudflareWARP") {
    $UseWarp = $true
}

# Auto-detect MEGAsync path if not specified
if ([string]::IsNullOrEmpty($MegasyncPath)) {
    $possiblePaths = @(
        "$env:LOCALAPPDATA\MEGAsync\MEGAsync.exe",
        "${env:ProgramFiles}\MEGAsync\MEGAsync.exe",
        "${env:ProgramFiles(x86)}\MEGAsync\MEGAsync.exe"
    )
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $MegasyncPath = $path
            break
        }
    }
    if ([string]::IsNullOrEmpty($MegasyncPath)) {
        $MegasyncPath = "$env:LOCALAPPDATA\MEGAsync\MEGAsync.exe"  # Default fallback
    }
}

# Global variables
$script:ShouldStop = $false
$script:CurrentState = "Disconnected"

#region Helper Functions

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('Info', 'Warning', 'Error', 'Success', 'Mega')]
        [string]$Level = 'Info'
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    $color = switch ($Level) {
        'Info' { 'White' }
        'Warning' { 'Yellow' }
        'Error' { 'Red' }
        'Success' { 'Green' }
        'Mega' { 'Cyan' }
    }
    
    Write-Host $logMessage -ForegroundColor $color
    Add-Content -Path $LogPath -Value $logMessage
}

function Test-VpnConnection {
    if ($UseWarp) {
        try {
            $status = warp-cli status
            $isConnected = $status -match "Status update: Connected"
            return @{ Exists = $true; Connected = $isConnected; Status = ($status -join " ") }
        }
        catch {
            return @{ Exists = $false; Connected = $false; Status = "WARP CLI Error" }
        }
    }
    else {
        try {
            $vpn = Get-VpnConnection -Name $VpnName -ErrorAction Stop
            return @{ Exists = $true; Connected = ($vpn.ConnectionStatus -eq 'Connected'); Status = $vpn.ConnectionStatus }
        }
        catch {
            return @{ Exists = $false; Connected = $false; Status = "Not Found" }
        }
    }
}

function Connect-VpnWithRetry {
    for ($i = 1; $i -le $MaxRetries; $i++) {
        try {
            Write-Log "Attempting to connect (Attempt $i of $MaxRetries)..." -Level Info
            
            if ($UseWarp) {
                # Ensure WARP UI is running
                if ($i -eq 1) { Start-WarpUi }
                warp-cli connect | Out-Null
            }
            else {
                rasdial $VpnName | Out-Null
            }
            
            Start-Sleep -Seconds 10
            $status = Test-VpnConnection
            if ($status.Connected) {
                Write-Log "Connected successfully!" -Level Success
                $script:CurrentState = "Connected"
                return $true
            }
        }
        catch {
            Write-Log "Connection failed: $($_.Exception.Message)" -Level Error
        }
        
        if ($i -lt $MaxRetries) { Start-Sleep -Seconds 10 }
    }
    return $false
}

function Disconnect-VpnWithRetry {
    for ($i = 1; $i -le $MaxRetries; $i++) {
        try {
            Write-Log "Attempting to disconnect (Attempt $i of $MaxRetries)..." -Level Info
            
            if ($UseWarp) {
                warp-cli disconnect | Out-Null
            }
            else {
                rasdial $VpnName /disconnect | Out-Null
            }
            
            Start-Sleep -Seconds 5
            $status = Test-VpnConnection
            if (-not $status.Connected) {
                Write-Log "Disconnected successfully!" -Level Success
                $script:CurrentState = "Disconnected"
                return $true
            }
        }
        catch {
            Write-Log "Disconnection failed: $($_.Exception.Message)" -Level Error
        }
        
        if ($i -lt $MaxRetries) { Start-Sleep -Seconds 5 }
    }
    return $false
}

function Restart-Megasync {
    Write-Log "Restarting MEGAsync..." -Level Mega
    
    # Kill process if running
    $process = Get-Process -Name "MEGAsync" -ErrorAction SilentlyContinue
    if ($process) {
        Write-Log "Stopping current MEGAsync process (ID: $($process.Id))..." -Level Mega
        Stop-Process -Name "MEGAsync" -Force -ErrorAction SilentlyContinue
        # Wait a bit for it to fully close
        Start-Sleep -Seconds 2
    }

    Write-Log "Waiting $MegasyncRestartDelaySeconds seconds before starting..." -Level Mega
    Start-Sleep -Seconds $MegasyncRestartDelaySeconds

    # Start process
    if (Test-Path $MegasyncPath) {
        Write-Log "Starting MEGAsync from $MegasyncPath" -Level Mega
        Start-Process -FilePath $MegasyncPath -WindowStyle Minimized
        Write-Log "MEGAsync started." -Level Success
    }
    else {
        Write-Log "Error: MEGAsync not found at $MegasyncPath" -Level Error
    }
}

function Start-WarpUi {
    # Check if WARP UI is already running
    $process = Get-Process -Name "Cloudflare WARP" -ErrorAction SilentlyContinue
    if ($process) {
        Write-Log "WARP UI is already running (PID: $($process.Id))" -Level Info
        return
    }

    # Start WARP UI if not running
    if (Test-Path $WarpUiPath) {
        Write-Log "Starting Cloudflare WARP UI from $WarpUiPath" -Level Info
        Start-Process -FilePath $WarpUiPath -WindowStyle Minimized
        Write-Log "WARP UI started." -Level Success
    }
    else {
        Write-Log "Warning: WARP UI not found at $WarpUiPath" -Level Warning
    }
}

function Wait-WithCountdown {
    param([int]$Minutes, [string]$NextAction)
    $totalSeconds = $Minutes * 60
    $endTime = (Get-Date).AddSeconds($totalSeconds)
    Write-Log "Waiting $Minutes minutes before: $NextAction" -Level Info
    while ((Get-Date) -lt $endTime -and -not $script:ShouldStop) {
        $remaining = ($endTime - (Get-Date))
        if ($remaining.TotalSeconds % 30 -lt 1) {
            Write-Host "`r[$(Get-Date -Format 'HH:mm:ss')] Current State: $script:CurrentState | Next Action: $NextAction in $([math]::Floor($remaining.TotalMinutes))m $($remaining.Seconds)s   " -NoNewline -ForegroundColor Cyan
        }
        Start-Sleep -Seconds 1
    }
    Write-Host ""
    return (-not $script:ShouldStop)
}

#endregion

# Main Loop
function Start-Main {
    Write-Host "`n=== VPN & MEGAsync Auto-Toggle ===" -ForegroundColor Cyan
    
    # Verify Initial State
    $status = Test-VpnConnection
    if (-not $status.Exists) {
        Write-Log "Error: VPN/Warp target not found." -Level Error
        return
    }

    $script:CurrentState = if ($status.Connected) { "Connected" } else { "Disconnected" }
    Write-Log "Initial State: $script:CurrentState" -Level Info

    while (-not $script:ShouldStop) {
        if ($script:CurrentState -eq "Disconnected") {
            if (Connect-VpnWithRetry) {
                Restart-Megasync
                if (-not (Wait-WithCountdown -Minutes $CycleDurationMinutes -NextAction "Disconnect")) { break }
            }
            else {
                Start-Sleep -Seconds 60
            }
        }
        else {
            if (Disconnect-VpnWithRetry) {
                Restart-Megasync
                if (-not (Wait-WithCountdown -Minutes $CycleDurationMinutes -NextAction "Connect")) { break }
            }
            else {
                Start-Sleep -Seconds 60
            }
        }
    }
    Write-Log "Script stopped." -Level Info
}

# Ctrl+C Handling
$script:ShouldStop = $false
try {
    [Console]::CancelKeyPress.Add({
            param($eventSender, $e)
            $e.Cancel = $true
            $script:ShouldStop = $true
            Write-Log "Shutdown signal received..." -Level Warning
        })
}
catch {}

# Execute
Start-Main
