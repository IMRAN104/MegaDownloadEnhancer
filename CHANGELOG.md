# Changelog

All notable changes to the VPN Auto-Toggle project will be documented in this file.

## [1.0.0] - 2024-01-15

### 🎉 Initial Release

#### Added
- **VPN-AutoToggle.ps1** - Main automation script with full functionality
  - Automatic VPN connection toggling (on/off cycles)
  - Configurable cycle duration (default: 10 minutes)
  - Robust error handling with retry logic
  - Comprehensive logging system
  - Real-time status display with countdown timer
  - Graceful shutdown on Ctrl+C
  - Support for both saved and explicit credentials
  - Windows native VPN management using rasdial

- **Test-VpnSetup.ps1** - System validation and testing tool
  - PowerShell version check
  - Administrator privilege detection
  - Execution policy verification
  - VPN module availability check
  - VPN connection enumeration
  - Specific VPN connection validation
  - rasdial command availability check
  - Script file existence verification

- **Documentation**
  - README.md - Comprehensive project overview
  - QUICK-START.md - Quick reference guide for immediate use
  - SETUP-AND-USAGE.md - Detailed setup instructions and troubleshooting
  - EXAMPLES.md - Real-world usage examples and scenarios
  - CHANGELOG.md - Version history (this file)

#### Features
- ✅ Automatic VPN cycling with configurable intervals
- ✅ Retry logic for failed connections (default: 3 attempts)
- ✅ Color-coded console output (Info/Warning/Error/Success)
- ✅ Persistent logging to file
- ✅ Live countdown display during wait periods
- ✅ Current state tracking (Connected/Disconnected)
- ✅ Cycle counter for monitoring
- ✅ Interrupt handling for clean shutdown
- ✅ Detailed error messages and recommendations
- ✅ Support for custom log paths
- ✅ Configurable maximum retry attempts

#### Parameters
- `VpnName` (Required) - Name of VPN connection
- `Username` (Optional) - VPN username
- `Password` (Optional) - VPN password
- `CycleDurationMinutes` (Optional, Default: 10) - Duration for each state
- `LogPath` (Optional, Default: .\VPN-AutoToggle.log) - Log file location
- `MaxRetries` (Optional, Default: 3) - Connection retry attempts

#### Technical Details
- PowerShell 5.1+ compatible
- Uses Windows built-in rasdial command
- Leverages Get-VpnConnection cmdlet for status checks
- Event-driven Ctrl+C handling
- Modular function design for maintainability

#### Testing
- Comprehensive test script included
- 8 validation checks covering all prerequisites
- Success rate calculation
- Detailed recommendations for failed tests
- Ready-to-use test commands

#### Documentation Highlights
- Quick start guide (3 steps to get running)
- 15+ usage examples covering common scenarios
- Troubleshooting guide with solutions
- Security best practices
- Task Scheduler integration instructions
- Real-time monitoring examples
- Log analysis commands

### Known Limitations
- Windows-only (uses Windows-specific VPN commands)
- Requires VPN connection to be pre-configured in Windows
- Administrator privileges recommended for best results
- Console window must remain open (or run as scheduled task)

### Future Considerations
- Cross-platform support (macOS, Linux)
- GUI interface option
- Email notifications for failures
- Multiple VPN support (rotate between different VPNs)
- Configuration file support
- Service/daemon mode
- Web dashboard for monitoring
- Statistics and reporting

---

## Version History

### Version Numbering
This project follows [Semantic Versioning](https://semver.org/):
- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality in a backward compatible manner
- **PATCH** version for backward compatible bug fixes

### Release Notes Format
- 🎉 Initial Release
- ✨ New Features
- 🐛 Bug Fixes
- 📝 Documentation
- ⚡ Performance Improvements
- 🔒 Security Updates
- ⚠️ Breaking Changes
- 🗑️ Deprecations

---

## Upgrade Guide

### From Future Versions
(This section will be populated when new versions are released)

---

## Contributing

When contributing to this project, please:
1. Update this CHANGELOG.md with your changes
2. Follow the existing format and emoji conventions
3. Place new entries under "Unreleased" section
4. Move to versioned section upon release

---

**Note**: This is the initial release (v1.0.0). Future updates will be documented here.

