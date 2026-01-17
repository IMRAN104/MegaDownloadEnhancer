# 🚀 GitHub Release Guide - VPN Manager

## Step-by-Step Guide to Publish VPN Manager on GitHub

---

## 📋 Prerequisites

- GitHub account with repository access
- Local machine with built application
- GitHub CLI (gh) or web browser

---

## 🔨 Step 1: Build Release Package

### Option A: Automated Build Script (Recommended)

```batch
cd VPNManager
release.bat
```

This will:
- ✅ Build the application in Release mode
- ✅ Create self-contained executable
- ✅ Copy all required files
- ✅ Create `Release-Package` folder with everything needed

### Option B: Manual Build

```batch
cd VPNManager
dotnet publish -c Release -r win-x64 --self-contained true
```

Then manually copy files from:
```
bin\Release\net10.0-windows\win-x64\publish\
```

To a new folder: `VPNManager-v1.2.0\`

---

## 📦 Step 2: Prepare Release Files

Make sure your release package contains:

### Required Files:
```
VPNManager-v1.2.0/
├── VPNManager.exe                 (Main application)
├── VPN-AutoToggle.ps1             (PowerShell script)
├── VPN-AutoToggle.log             (Will be created at runtime)
├── vpn-settings.json              (Will be created at runtime)
├── QUICKSTART.txt                 (User guide)
└── Documentation/
    ├── README.md
    ├── INSTALL.md
    ├── WARP.md
    └── CLOUDFLARE_WARP_GUIDE.md
```

### Create ZIP Archive:
```batch
# Using PowerShell
Compress-Archive -Path "VPNManager-v1.2.0\*" -DestinationPath "VPNManager-v1.2.0-win-x64.zip"
```

Or use File Explorer:
1. Right-click `VPNManager-v1.2.0` folder
2. Send to → Compressed (zipped) folder

---

## 🌐 Step 3: Create GitHub Release

### Option A: Using GitHub Web Interface

1. **Go to GitHub Releases**
   ```
   https://github.com/YOUR_USERNAME/MegaDownloadEnhancer/releases
   ```

2. **Click "Draft a new release"**

3. **Fill in release details:**
   - **Tag version:** `v1.2.0`
   - **Release title:** `VPN Manager v1.2.0 - Feature Enhancements & Critical Fixes`
   - **Description:** (use the template below)

4. **Upload files:**
   - Drag and drop `VPNManager-v1.2.0-win-x64.zip`
   - Or click "Attach binaries" and select the ZIP file

5. **Click "Publish release"**

### Option B: Using GitHub CLI

```batch
gh release create v1.2.0 ^
  --title "VPN Manager v1.2.0" ^
  --notes-file RELEASE_NOTES.md ^
  VPNManager-v1.2.0-win-x64.zip
```

---

## 📝 Step 4: Release Description Template

Copy and paste this into your GitHub release description:

```markdown
# 🎉 VPN Manager v1.2.0 - Feature Enhancements & Critical Fixes

## 🚀 What's New

### ✨ New Features
- **Single Toggle Button** - Start/Stop cycle with one button
- **Real-Time Status Monitoring** - Live VPN and MEGAsync status updates
- **Cycle Start Time Display** - See when the cycle started
- **Configurable Refresh Interval** - Update status every 0.5-60 seconds
- **Custom Process Support** - Monitor any process, not just MEGAsync

### 🛠️ Improvements
- **Standardized WARP Naming** - Removed duplicate "WARP" option, kept "CloudflareWARP"
- **Enhanced Stop Cycle** - Now only disconnects VPN without restarting MEGAsync
- **Better Architecture** - VpnService now properly manages VPN connections
- **Improved Error Handling** - Cleaner process management and disconnection

### 🐛 Bug Fixes
- **Fixed Stop Cycle** - WARP now properly disconnects when stopping cycle
- **Fixed PowerShell Integration** - Settings properly passed via JSON
- **Fixed Process Cleanup** - No orphaned processes after stopping
- **Fixed PSScriptAnalyzer Warnings** - All PowerShell warnings resolved

## 📦 Installation

### Quick Start
1. Download `VPNManager-v1.2.0-win-x64.zip`
2. Extract to a folder (e.g., `C:\Program Files\VPNManager`)
3. Run `VPNManager.exe`
4. Configure your VPN settings
5. Click "Start Cycle"

### Requirements
- Windows 10/11
- Cloudflare WARP (for WARP support)
- No .NET installation needed (self-contained)

### First-Time Setup
1. Install Cloudflare WARP from [1.1.1.1](https://1.1.1.1/)
2. Run VPNManager.exe
3. Open Settings → Select "CloudflareWARP"
4. Set cycle duration (default: 10 minutes)
5. Click OK → Start Cycle

## 📖 Documentation

- **QUICKSTART.txt** - Quick start guide
- **INSTALL.md** - Detailed installation instructions
- **WARP.md** - WARP setup and configuration
- **CLOUDFLARE_WARP_GUIDE.md** - Complete WARP guide

## 🐛 Known Issues

None at this time. Please report issues on GitHub Issues.

## 📊 System Requirements

- **OS:** Windows 10/11 or Windows Server 2016+
- **RAM:** 50MB typical usage
- **Disk:** 50MB free space
- **Network:** Internet connection for VPN

## 🔄 Upgrading

From v1.1 or earlier:
1. Close old VPNManager
2. Extract new version to same folder
3. Run VPNManager.exe
4. Settings are automatically preserved

## 💬 Support

- 📖 Check documentation files
- 🐛 Report issues: [GitHub Issues](https://github.com/YOUR_USERNAME/MegaDownloadEnhancer/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/YOUR_USERNAME/MegaDownloadEnhancer/discussions)

## 📜 Changelog

### v1.2.0 (2026-01-18)
- ✅ Standardized WARP naming
- ✅ Fixed Stop Cycle behavior
- ✅ Enhanced VpnService architecture
- ✅ Improved error handling
- ✅ Fixed all PSScriptAnalyzer warnings

### v1.1.0
- Initial Windows Forms release
- Real-time status monitoring
- System tray support

## 🙏 Credits

Built with:
- .NET 10.0
- Windows Forms
- Cloudflare WARP
- PowerShell 5.1+

---

**Download:** `VPNManager-v1.2.0-win-x64.zip` (Self-contained, no dependencies needed)

**Checksum:** (Generate with: `certutil -hashfile VPNManager-v1.2.0-win-x64.zip SHA256`)
```
SHA256: [YOUR_CHECKSUM_HERE]
```
```

---

## 🔐 Step 5: Generate Checksum (Optional but Recommended)

This helps users verify the download integrity:

```batch
certutil -hashfile VPNManager-v1.2.0-win-x64.zip SHA256
```

Add the output to your release description.

---

## 📢 Step 6: Announce Your Release

### Update README.md

Add to your main README:

```markdown
## 📥 Download

### Latest Release: v1.2.0

[🚀 Download VPN Manager v1.2.0](https://github.com/YOUR_USERNAME/MegaDownloadEnhancer/releases/latest)

**Features:**
- ✅ Automatic VPN cycling
- ✅ Real-time status monitoring
- ✅ MEGAsync integration
- ✅ System tray support

**Requirements:**
- Windows 10/11
- Cloudflare WARP

### Quick Install

1. [Download latest release](https://github.com/YOUR_USERNAME/MegaDownloadEnhancer/releases/latest)
2. Extract to folder
3. Run `VPNManager.exe`
4. Configure and start!

See [INSTALL.md](INSTALL.md) for detailed instructions.
```

---

## 🎯 Step 7: Tagging the Release (If not done automatically)

```batch
# Create and push tag
git tag -a v1.2.0 -m "VPN Manager v1.2.0 - Feature Enhancements"
git push origin v1.2.0

# Or use GitHub CLI
gh release create v1.2.0 ./VPNManager-v1.2.0-win-x64.zip ^
  --title "VPN Manager v1.2.0" ^
  --notes "See RELEASE_NOTES.md for details"
```

---

## 📊 Step 8: Monitor and Respond

After publishing:

1. **Monitor Issues** - Respond to user feedback
2. **Track Downloads** - Check GitHub Analytics
3. **Update Docs** - Add FAQs based on common questions
4. **Fix Bugs** - Release patch versions if needed

---

## 🔄 Future Releases

### Version Numbering
- **Major.Minor.Patch** (e.g., 1.2.0)
- Major: Breaking changes
- Minor: New features
- Patch: Bug fixes

### Release Workflow
1. Develop on `feature-*` branches
2. Test thoroughly
3. Merge to `main`
4. Create release tag
5. Build release package
6. Publish GitHub release
7. Announce to users

---

## ✅ Checklist

Before publishing:

- [ ] Application builds without errors
- [ ] All features tested manually
- [ ] Documentation complete
- [ ] Release notes written
- [ ] ZIP archive created
- [ ] Checksum generated
- [ ] README updated with download link
- [ ] GitHub release created
- [ ] Files uploaded
- [ ] Release published

---

## 🎉 Congratulations!

Your VPN Manager is now live on GitHub! Users can download and install it easily.

**Next Steps:**
- Monitor user feedback
- Fix any reported issues
- Plan next version features
- Consider creating a wiki for documentation

---

**Need Help?**
- Check [GitHub's Release Documentation](https://docs.github.com/en/repositories/releasing-projects-on-github)
- Review [GitHub CLI Documentation](https://cli.github.com/manual/gh_release)
