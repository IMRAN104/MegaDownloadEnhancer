@echo off
REM ============================================
REM VPN Manager - Release Build Script
REM Creates a distributable release package
REM ============================================

echo ========================================
echo VPN Manager - Release Build
echo ========================================
echo.

REM Check if .NET SDK is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 10.0 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
dotnet clean VPNManager.csproj -c Release -v minimal

echo.
echo [2/5] Building Release configuration...
dotnet publish VPNManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo.
echo [3/5] Creating release package...

REM Define paths
set "SOURCE_DIR=bin\Release\net10.0-windows\win-x64\publish"
set "RELEASE_DIR=Release-Package"
set "VERSION=1.2.0"

REM Clean and create release directory
if exist "%RELEASE_DIR%" rmdir /s /q "%RELEASE_DIR%"
mkdir "%RELEASE_DIR%"

REM Copy application files
echo Copying application files...
xcopy "%SOURCE_DIR%\*" "%RELEASE_DIR%\" /E /I /Y

REM Copy required files that should be in the same directory
echo Copying PowerShell script and documentation...
copy "..\VPN-AutoToggle.ps1" "%RELEASE_DIR%\" >nul
copy "..\README.md" "%RELEASE_DIR%\" >nul
copy "..\INSTALL.md" "%RELEASE_DIR%\" >nul
copy "..\WARP.md" "%RELEASE_DIR%\" >nul
copy "..\CLOUDFLARE_WARP_GUIDE.md" "%RELEASE_DIR%\" >nul

REM Create user guide
echo Creating user guide...
(
echo # VPN Manager - Quick Start Guide
echo.
echo ## Installation
echo.
echo 1. Extract all files to a folder (e.g., C:\Program Files\VPNManager)
echo 2. Run VPNManager.exe
echo 3. Configure your VPN settings in Settings
echo 4. Click "Start Cycle" to begin
echo.
echo ## Requirements
echo.
echo - Windows 10/11
echo - Cloudflare WARP (for WARP support)
echo - .NET 10.0 Runtime (included in self-contained build)
echo.
echo ## First Time Setup
echo.
echo 1. Install Cloudflare WARP from: https://1.1.1.1/
echo 2. Run VPNManager.exe
echo 3. Open Settings
echo 4. Select "CloudflareWARP" from VPN dropdown
echo 5. Set cycle duration (default: 10 minutes)
echo 6. Click OK
echo 7. Click "Start Cycle"
echo.
echo ## Features
echo.
echo - Automatic VPN cycling every N minutes
echo - Real-time status monitoring
echo - MEGAsync auto-restart on VPN toggle
echo - System tray support
echo - Persistent settings
echo.
echo ## Troubleshooting
echo.
echo If WARP doesn't connect:
echo - Open WARP desktop app and sign in
echo - Check firewall settings
echo - Run VPNManager as Administrator
echo.
echo For more information, see the included documentation files.
echo.
echo ## Version
echo.
echo VPN Manager v%VERSION%
echo Release Date: %date%
) > "%RELEASE_DIR%\QUICKSTART.txt"

echo.
echo [4/5] Creating ZIP archive...
set "ZIP_NAME=VPNManager-v%VERSION%-win-x64"

REM Create a temporary list of files to zip
set "TEMP_LIST=%TEMP%\zip_list.txt"
dir /b /s "%RELEASE_DIR%" > "%TEMP_LIST%"

echo.
echo [5/5] Build complete!
echo.
echo ========================================
echo Release Package Created
echo ========================================
echo.
echo Location: %CD%\%RELEASE_DIR%\
echo Version: %VERSION%
echo.
echo To distribute:
echo 1. Upload files from: %RELEASE_DIR%\
echo 2. Or create a ZIP manually from: %RELEASE_DIR%\
echo.
echo Files ready for GitHub Release:
echo - VPNManager.exe (main application)
echo - VPN-AutoToggle.ps1 (PowerShell script)
echo - Documentation files
echo.
echo ========================================
echo Next Steps:
echo ========================================
echo.
echo 1. Test the application:
echo    cd "%RELEASE_DIR%"
echo    VPNManager.exe
echo.
echo 2. Create GitHub Release:
echo    - Go to: https://github.com/YOUR_USERNAME/MegaDownloadEnhancer/releases
echo    - Click "Draft a new release"
echo    - Tag: v%VERSION%
echo    - Title: VPN Manager v%VERSION%
echo    - Upload: %RELEASE_DIR%\
echo    - Publish release
echo.
pause
