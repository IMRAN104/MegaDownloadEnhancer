# 🚀 CI/CD Guide - VPN Manager

This guide explains the automated CI/CD process for the VPN Manager project using GitHub Actions.

## Overview

The project uses GitHub Actions for automated building, testing, and releasing. This eliminates manual build steps and ensures consistent, reliable releases.

## Workflows

### 1. Build and Test (`build-test.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main`
- Manual trigger via GitHub UI

**What it does:**
- ✅ Builds the application on Windows runners
- ✅ Runs all PowerShell tests
- ✅ Creates build artifacts for testing
- ✅ Validates code quality

**Where to view:** [Actions Tab](../../actions/workflows/build-test.yml)

---

### 2. Release (`release.yml`)

**Triggers:**
- Push tags matching `v*.*.*` pattern (e.g., `v1.2.0`)
- Manual trigger via GitHub UI (requires version input)

**What it does:**
- ✅ Builds release package
- ✅ Creates ZIP archive
- ✅ Generates SHA256 checksum
- ✅ Creates GitHub release
- ✅ Uploads release assets
- ✅ Generates release notes

**Where to view:** [Actions Tab](../../actions/workflows/release.yml)

---

## Creating a New Release

### Automated Way (Recommended)

1. **Update version in code** (if needed)
   
2. **Create and push a version tag:**
   ```bash
   git tag v1.3.0
   git push origin v1.3.0
   ```

3. **That's it!** GitHub Actions will:
   - Build the application
   - Create release package
   - Generate checksums
   - Create GitHub release
   - Upload all assets

4. **Check the release:** Go to [Releases](../../releases) to see your new release!

### Manual Trigger

If you prefer manual control:

1. Go to [Actions > Release](../../actions/workflows/release.yml)
2. Click **"Run workflow"**
3. Enter the version (e.g., `1.3.0`)
4. Click **"Run workflow"**

---

## Workflow Status Badges

Add these badges to your README to show build status:

### Build Status
```markdown
![Build Status](https://github.com/IMRAN104/MegaDownloadEnhancer/actions/workflows/build-test.yml/badge.svg)
```

### Latest Release
```markdown
![Latest Release](https://img.shields.io/github/v/release/IMRAN104/MegaDownloadEnhancer)
```

### Download Count
```markdown
![Downloads](https://img.shields.io/github/downloads/IMRAN104/MegaDownloadEnhancer/total)
```

---

## Workflow Details

### Build Process

The workflows use the following build configuration:

```yaml
Platform: Windows (windows-latest runner)
.NET SDK: 10.0.x
Runtime: win-x64
Build Type: Self-contained
Output: Single executable with dependencies
```

### Release Package Contents

Each release includes:
- `VPNManager.exe` - Main application
- `VPN-AutoToggle.ps1` - PowerShell script
- `QUICKSTART.txt` - Quick start guide
- `README.md` - Full documentation
- Additional documentation files (WARP.md, CHANGELOG.md, etc.)

### Checksum Verification

Every release includes a `checksum.txt` file with SHA256 hash. Users can verify:

```powershell
Get-FileHash VPNManager-v1.3.0-win-x64.zip -Algorithm SHA256
```

---

## Troubleshooting

### Build Fails

1. **Check the Actions tab** for error logs
2. **Review the build output** - click on the failed workflow
3. **Common issues:**
   - .NET SDK version mismatch
   - Missing dependencies
   - Syntax errors in code

### Release Not Created

1. **Verify tag format:** Must be `v*.*.*` (e.g., `v1.2.0`)
2. **Check permissions:** Ensure `GITHUB_TOKEN` has write access
3. **Review workflow logs** in the Actions tab

### Tests Failing

1. **Local testing:** Run tests locally first
   ```powershell
   ./Test-E2E-Integration.ps1
   ./Test-VpnSetup.ps1
   ```
2. **CI environment:** Tests may need VPN/MEGAsync - currently configured to not fail build
3. **Update tests** if needed for CI environment

---

## Local Testing (Optional)

You can test the workflow locally using [act](https://github.com/nektos/act):

```bash
# Install act
winget install nektos.act

# Run build workflow
act -W .github/workflows/build-test.yml

# Run release workflow
act -W .github/workflows/release.yml --secret GITHUB_TOKEN=<your-token>
```

---

## Migration from Manual Process

### Old Process (Manual)
```batch
cd VPNManager
release.bat
# ... manual steps ...
# Upload to GitHub
```

### New Process (Automated)
```bash
git tag v1.3.0
git push origin v1.3.0
# Done! ✅
```

The `release.bat` file remains in the repository as a backup, but is no longer needed for releases.

---

## Benefits

✅ **Consistency** - Every build uses the same environment
✅ **Speed** - Releases in minutes instead of manual steps
✅ **Reliability** - Eliminates human error
✅ **Traceability** - Full audit trail of all builds
✅ **Transparency** - Anyone can see build status
✅ **Automation** - Set it and forget it

---

## Advanced Configuration

### Customizing Workflows

Workflows are in `.github/workflows/`:
- `build-test.yml` - CI workflow
- `release.yml` - Release workflow

Edit these files to customize behavior.

### Environment Variables

Set in GitHub repository settings under **Settings > Secrets and variables > Actions**:
- `GITHUB_TOKEN` - Automatically provided by GitHub
- Add custom secrets as needed

### Version Numbering

Follow [Semantic Versioning](https://semver.org/):
- **Major.Minor.Patch** (e.g., `1.2.0`)
- **Major:** Breaking changes
- **Minor:** New features
- **Patch:** Bug fixes

---

## Support

- **Issues:** [GitHub Issues](../../issues)
- **Discussions:** [GitHub Discussions](../../discussions)
- **GitHub Actions Docs:** [docs.github.com/actions](https://docs.github.com/actions)

---

## Quick Reference

| Task | Command |
|------|---------|
| Create release | `git tag v1.3.0 && git push origin v1.3.0` |
| View workflows | Go to Actions tab |
| Download artifacts | Go to workflow run → Artifacts section |
| View releases | Go to Releases page |
| Cancel workflow | Actions tab → Running workflow → Cancel |

---

**Happy releasing! 🎉**
