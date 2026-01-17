# GitHub Actions Workflow Validation Checklist

This document provides a checklist to validate the GitHub Actions workflows before pushing to the repository.

## Pre-Deployment Checklist

### Workflow Files

- [x] `build-test.yml` created in `.github/workflows/`
- [x] `release.yml` created in `.github/workflows/`
- [ ] Workflow YAML syntax is valid (check with YAML linter)
- [ ] All required secrets are documented

### Configuration

- [x] .NET version specified: 10.0.x
- [x] Windows runner specified: `windows-latest`
- [x] Build configuration set to Release
- [x] Self-contained build enabled
- [x] Runtime identifier: win-x64

### Build Workflow (`build-test.yml`)

- [x] Triggers on push to main/develop
- [x] Triggers on pull requests to main
- [x] Manual trigger enabled (workflow_dispatch)
- [x] .NET SDK setup step included
- [x] Restore dependencies step included
- [x] Build step included (Debug & Release)
- [x] Test execution step included
- [x] Artifacts upload configured
- [x] Appropriate retention period set (7 days)

### Release Workflow (`release.yml`)

- [x] Triggers on version tags (v*.*.*)
- [x] Manual trigger enabled with version input
- [x] Write permissions for contents
- [x] Version extraction from tag
- [x] Complete build process included
- [x] Release package creation
- [x] ZIP archive generation
- [x] SHA256 checksum calculation
- [x] Release notes generation
- [x] GitHub release creation
- [x] Asset upload to release

### Documentation

- [x] `CI_CD_GUIDE.md` created with complete instructions
- [x] README updated with badges
- [x] README updated with download section
- [x] README updated with CI/CD information
- [x] Project files table updated

### Project Configuration

- [x] `VPNManager.csproj` updated with version 1.2.0
- [x] Repository URL added to csproj
- [x] Package metadata added

## Testing Before First Release

### Local Validation

1. **Test YAML Syntax:**
   ```bash
   # Install yamllint (if available)
   pip install yamllint
   yamllint .github/workflows/*.yml
   ```

2. **Verify File Paths:**
   - [ ] Check all file paths in workflows are correct
   - [ ] Verify PowerShell script paths
   - [ ] Verify documentation file paths

3. **Test Build Locally:**
   ```powershell
   # Test the build command
   dotnet publish VPNManager/VPNManager.csproj -c Release -r win-x64 --self-contained true
   ```

### First Push Checklist

1. **Push to Feature Branch First:**
   ```bash
   git checkout -b ci/add-github-actions
   git add .github/ CI_CD_GUIDE.md readme.md VPNManager/VPNManager.csproj
   git commit -m "Add GitHub Actions CI/CD automation"
   git push origin ci/add-github-actions
   ```

2. **Create Pull Request:**
   - Create PR to main branch
   - Verify build-test workflow runs
   - Check for any errors in Actions tab

3. **Review Workflow Run:**
   - [ ] Build completes successfully
   - [ ] Tests execute (may not all pass without VPN)
   - [ ] Artifacts are created
   - [ ] No syntax errors in workflow

### First Release Test

1. **Create Test Tag:**
   ```bash
   git checkout main
   git pull origin main
   git tag v1.2.0-test
   git push origin v1.2.0-test
   ```

2. **Verify Release Workflow:**
   - [ ] Workflow triggers automatically
   - [ ] Build completes successfully
   - [ ] ZIP file is created
   - [ ] Checksum is calculated
   - [ ] Release is created on GitHub
   - [ ] Assets are uploaded to release
   - [ ] Release notes are generated

3. **Download and Test:**
   - [ ] Download the ZIP from releases
   - [ ] Extract the ZIP
   - [ ] Run VPNManager.exe
   - [ ] Verify all files are included
   - [ ] Verify checksum matches

### Post-Deployment

1. **Monitor First Few Releases:**
   - Watch for build failures
   - Check artifact sizes
   - Verify download counts

2. **Document Issues:**
   - Note any problems in CI_CD_GUIDE.md
   - Update troubleshooting section

3. **Cleanup Test Releases:**
   ```bash
   # Delete test tag after validation
   git tag -d v1.2.0-test
   git push origin :refs/tags/v1.2.0-test
   ```

## Common Issues and Solutions

### Issue: Workflow doesn't trigger

**Solution:**
- Ensure `.github/workflows/` directory is in repository root
- Check YAML syntax is valid
- Verify branch names in trigger configuration

### Issue: Build fails

**Solution:**
- Check .NET SDK version matches project
- Verify all dependencies are restored
- Check file paths are correct

### Issue: Release not created

**Solution:**
- Verify `GITHUB_TOKEN` has write permissions
- Check tag format matches pattern `v*.*.*`
- Review release workflow logs

### Issue: Tests fail in CI

**Solution:**
- Tests may require VPN/MEGAsync which aren't installed in CI
- Current configuration allows tests to fail without breaking build
- Consider mocking tests or using conditional execution

## Security Checklist

- [x] No hardcoded credentials in workflows
- [x] Using GitHub-provided `GITHUB_TOKEN`
- [ ] Repository secrets configured (if needed)
- [x] Workflows only run on trusted branches/tags

## Success Criteria

✅ All workflows created and configured
✅ Documentation complete
✅ README updated
✅ Project metadata updated
⏳ Workflows validated (requires push to GitHub)
⏳ First release tested (requires tag push)

---

**Next Steps:**
1. Review all created files
2. Commit and push to feature branch
3. Create PR and test build workflow
4. Merge to main
5. Create test release tag
6. Validate complete release process
7. Create official v1.2.0 release
