@echo off
echo ====================================
echo Building VPN Manager Application
echo ====================================
echo.

REM Check if .NET SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed!
    echo Please install .NET 6.0 SDK or later from:
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo [1/3] Restoring NuGet packages...
dotnet restore VPNManager.csproj
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages!
    pause
    exit /b 1
)

echo.
echo [2/3] Building application...
dotnet build VPNManager.csproj -c Release
if %errorlevel% neq 0 (
    echo ERROR: Failed to build!
    pause
    exit /b 1
)

echo.
echo [3/3] Copying PowerShell script to output...
if not exist "..\VPN-AutoToggle.ps1" (
    echo WARNING: VPN-AutoToggle.ps1 not found in parent directory!
    echo Please ensure the PowerShell script is in the project root.
) else (
    copy "..\VPN-AutoToggle.ps1" "bin\Release\net6.0-windows\" >nul
    echo PowerShell script copied successfully.
)

echo.
echo ====================================
echo Build completed successfully!
echo ====================================
echo.
echo Output location: bin\Release\net6.0-windows\
echo Executable: VPNManager.exe
echo.
echo To run the application:
echo   cd bin\Release\net6.0-windows\
echo   VPNManager.exe
echo.

pause
