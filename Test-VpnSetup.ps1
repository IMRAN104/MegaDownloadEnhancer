<#
.SYNOPSIS
    VPN Setup Test and Validation Script

.DESCRIPTION
    This script helps you verify that your system is ready to run the VPN Auto-Toggle script.
    It checks prerequisites, tests VPN connectivity, and validates the environment.

.PARAMETER VpnName
    The name of the VPN connection to test

.EXAMPLE
    .\Test-VpnSetup.ps1 -VpnName "MyVPN"

.NOTES
    Run this script before using VPN-AutoToggle.ps1 to ensure everything is configured correctly.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$VpnName = "",

    [Parameter(Mandatory=$false)]
    [switch]$SkipAdminCheck,

    [Parameter(Mandatory=$false)]
    [switch]$SkipVpnCheck
)

$script:TestResults = @()
$script:PassCount = 0
$script:FailCount = 0

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message,
        [string]$Recommendation = ""
    )

    $symbol = if ($Passed) { "[PASS]" } else { "[FAIL]" }
    $color = if ($Passed) { "Green" } else { "Red" }

    Write-Host "`n[$symbol] $TestName" -ForegroundColor $color
    Write-Host "    $Message" -ForegroundColor Gray

    if ($Recommendation -and $Recommendation -ne "") {
        Write-Host "    >> $Recommendation" -ForegroundColor Yellow
    }

    $script:TestResults += [PSCustomObject]@{
        Test = $TestName
        Passed = $Passed
        Message = $Message
        Recommendation = $Recommendation
    }

    if ($Passed) { $script:PassCount++ } else { $script:FailCount++ }
}

function Test-Prerequisites {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  VPN Setup Test & Validation" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan

    # Test 1: PowerShell Version
    Write-Host "[1/8] Checking PowerShell version..." -ForegroundColor Cyan
    $psVersion = $PSVersionTable.PSVersion
    $versionOk = $psVersion.Major -ge 5
    Write-TestResult `
        -TestName "PowerShell Version" `
        -Passed $versionOk `
        -Message "Version: $($psVersion.Major).$($psVersion.Minor)" `
        -Recommendation $(if (-not $versionOk) { "Upgrade to PowerShell 5.1 or higher" } else { "" })

    # Test 2: Administrator Privileges
    if ($SkipAdminCheck) {
        Write-Host "`n[2/8] Skipping administrator privileges check (-SkipAdminCheck)" -ForegroundColor Yellow
    }
    else {
        Write-Host "`n[2/8] Checking administrator privileges..." -ForegroundColor Cyan
        $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
        Write-TestResult `
            -TestName "Administrator Privileges" `
            -Passed $isAdmin `
            -Message $(if ($isAdmin) { "Running as Administrator" } else { "Not running as Administrator" }) `
            -Recommendation $(if (-not $isAdmin) { "Run PowerShell as Administrator for best results" } else { "" })
    }

    # Test 3: Execution Policy
    Write-Host "`n[3/8] Checking execution policy..." -ForegroundColor Cyan
    $execPolicy = Get-ExecutionPolicy -Scope CurrentUser
    $policyOk = $execPolicy -in @('RemoteSigned', 'Unrestricted', 'Bypass')
    Write-TestResult `
        -TestName "Execution Policy" `
        -Passed $policyOk `
        -Message "Current policy: $execPolicy" `
        -Recommendation $(if (-not $policyOk) { "Run: Set-ExecutionPolicy RemoteSigned -Scope CurrentUser" } else { "" })

    # Test 4: VPN Module
    Write-Host "`n[4/8] Checking VPN PowerShell module..." -ForegroundColor Cyan
    try {
        $null = Get-Command Get-VpnConnection -ErrorAction Stop
        Write-TestResult `
            -TestName "VPN Module" `
            -Passed $true `
            -Message "VPN cmdlets available"
    }
    catch {
        Write-TestResult `
            -TestName "VPN Module" `
            -Passed $false `
            -Message "VPN cmdlets not found" `
            -Recommendation "Ensure you're running on Windows 10/11 or Windows Server with VPN features"
    }

    # Test 5: List VPN Connections
    if ($SkipVpnCheck) {
        Write-Host "`n[5/8] Skipping VPN connections check (-SkipVpnCheck)" -ForegroundColor Yellow
    }
    else {
    Write-Host "`n[5/8] Checking for VPN connections..." -ForegroundColor Cyan
    try {
        $vpnConnections = Get-VpnConnection -ErrorAction Stop
        $hasVpn = $vpnConnections.Count -gt 0

        if ($hasVpn) {
            Write-TestResult `
                -TestName "VPN Connections Found" `
                -Passed $true `
                -Message "Found $($vpnConnections.Count) VPN connection(s)"

            Write-Host "`n    Available VPN Connections:" -ForegroundColor Gray
            $vpnConnections | ForEach-Object {
                Write-Host "      - $($_.Name) [$($_.ConnectionStatus)]" -ForegroundColor Gray
            }
        }
        else {
            Write-TestResult `
                -TestName "VPN Connections Found" `
                -Passed $false `
                -Message "No VPN connections configured" `
                -Recommendation "Create a VPN connection in Windows Settings → Network & Internet → VPN"
        }
    }
    catch {
        Write-TestResult `
            -TestName "VPN Connections Found" `
            -Passed $false `
            -Message "Error checking VPN connections: $($_.Exception.Message)" `
            -Recommendation "Verify VPN features are installed on your system"
    }
    }  # end else (-SkipVpnCheck)

    # Test 6: Specific VPN Connection (if provided)
    if ($VpnName) {
        Write-Host "`n[6/8] Checking specific VPN connection '$VpnName'..." -ForegroundColor Cyan
        try {
            $vpn = Get-VpnConnection -Name $VpnName -ErrorAction Stop
            Write-TestResult `
                -TestName "VPN Connection '$VpnName'" `
                -Passed $true `
                -Message "Status: $($vpn.ConnectionStatus), Server: $($vpn.ServerAddress)"
        }
        catch {
            Write-TestResult `
                -TestName "VPN Connection '$VpnName'" `
                -Passed $false `
                -Message "VPN connection not found" `
                -Recommendation "Use exact name from the list above or create the VPN connection"
        }
    }
    else {
        Write-Host "`n[6/8] Skipping specific VPN test (no VPN name provided)" -ForegroundColor Yellow
    }

    # Test 7: rasdial Command
    Write-Host "`n[7/8] Checking rasdial command availability..." -ForegroundColor Cyan
    try {
        $null = rasdial 2>&1
        Write-TestResult `
            -TestName "rasdial Command" `
            -Passed $true `
            -Message "rasdial is available"
    }
    catch {
        Write-TestResult `
            -TestName "rasdial Command" `
            -Passed $false `
            -Message "rasdial command not found" `
            -Recommendation "rasdial should be available on all Windows systems. Check system PATH."
    }

    # Test 8: Script File Exists
    Write-Host "`n[8/8] Checking for VPN-AutoToggle.ps1 script..." -ForegroundColor Cyan
    $scriptPath = Join-Path $PSScriptRoot "VPN-AutoToggle.ps1"
    $scriptExists = Test-Path $scriptPath
    Write-TestResult `
        -TestName "VPN-AutoToggle.ps1 Script" `
        -Passed $scriptExists `
        -Message $(if ($scriptExists) { "Found at: $scriptPath" } else { "Not found in current directory" }) `
        -Recommendation $(if (-not $scriptExists) { "Ensure VPN-AutoToggle.ps1 is in the same folder as this test script" } else { "" })
}

function Show-Summary {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  Test Summary" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan

    $totalTests = $script:PassCount + $script:FailCount
    $passPercent = if ($totalTests -gt 0) { [math]::Round(($script:PassCount / $totalTests) * 100, 1) } else { 0 }

    Write-Host "Total Tests: $totalTests" -ForegroundColor White
    Write-Host "Passed: $script:PassCount" -ForegroundColor Green
    Write-Host "Failed: $script:FailCount" -ForegroundColor Red
    Write-Host "Success Rate: $passPercent%" -ForegroundColor $(if ($passPercent -ge 80) { "Green" } else { "Yellow" })

    if ($script:FailCount -eq 0) {
        Write-Host "`n[SUCCESS] All tests passed! You're ready to use VPN-AutoToggle.ps1" -ForegroundColor Green

        if ($VpnName) {
            Write-Host "`nRecommended command to start:" -ForegroundColor Cyan
            Write-Host "  .\VPN-AutoToggle.ps1 -VpnName `"$VpnName`" -CycleDurationMinutes 1" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "`n[WARNING] Some tests failed. Please address the recommendations above." -ForegroundColor Yellow
    }
}

# Run all tests
Test-Prerequisites
Show-Summary

Write-Host "`n" -NoNewline

