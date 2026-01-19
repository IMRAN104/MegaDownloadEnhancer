# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.2.x   | :white_check_mark: |
| 1.1.x   | :white_check_mark: |
| < 1.1   | :x:                |

## Reporting a Vulnerability

We take the security of MegaDownloadEnhancer seriously. If you discover a security vulnerability, please follow these steps:

### Where to Report

**Please DO NOT open a public issue.** Instead, report security vulnerabilities via one of these methods:

1. **GitHub Security Advisories** (Preferred)
   - Navigate to the [Security tab](https://github.com/IMRAN104/MegaDownloadEnhancer/security)
   - Click "Report a vulnerability"
   - Fill out the private vulnerability report form

2. **Email** (Alternative)
   - Contact the maintainers directly via email (create a GitHub issue asking for security contact email)
   - Include "SECURITY" in the subject line
   - Provide detailed information about the vulnerability

### What to Include

When reporting a vulnerability, please include:

- **Description** - Clear description of the vulnerability
- **Impact** - What an attacker could potentially do
- **Steps to Reproduce** - Detailed steps to reproduce the issue
- **Affected Versions** - Which versions are vulnerable
- **Suggested Fix** - If you have ideas for how to fix it (optional)
- **Proof of Concept** - Code or screenshots demonstrating the issue (if applicable)

### Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Timeline**: Depends on severity
  - Critical: 1-7 days
  - High: 7-14 days
  - Medium: 14-30 days
  - Low: 30-90 days

### Security Best Practices for Users

When using MegaDownloadEnhancer, follow these security guidelines:

#### VPN Credential Management

✅ **DO:**
- Save VPN credentials in Windows Credential Manager
- Use the VPNManager GUI settings (stored securely in `%AppData%\VPNManager\settings.json`)
- Use PowerShell script with command-line parameters only

❌ **DON'T:**
- Hardcode passwords in scripts or configuration files
- Share your configuration files publicly
- Commit `settings.json` or log files to version control
- Pass passwords via command-line (visible in process list)

#### General Security

✅ **DO:**
- Keep the application updated to the latest version
- Review the CHANGELOG for security updates
- Run with appropriate permissions (avoid unnecessary admin rights)
- Verify downloaded releases using SHA256 checksums

❌ **DON'T:**
- Download builds from unofficial sources
- Disable Windows security features to run the app
- Share your VPNManager settings folder with others

#### Log File Security

Log files may contain:
- VPN connection timestamps
- System information
- Error messages with file paths

**Recommendations:**
- Regularly review and clean up old log files
- Do not share log files publicly without redacting sensitive information
- Store log files in secure locations

### Known Security Considerations

1. **VPN Credentials**: This application may handle VPN credentials. Always use Windows Credential Manager to save credentials rather than passing them as parameters.

2. **Process Manipulation**: The application can start/stop processes (MEGAsync, VPN). Ensure you trust the VPN and applications being managed.

3. **Administrator Privileges**: Some VPN operations may require administrator privileges. Only grant these when necessary.

4. **Log Files**: Log files may contain sensitive information (paths, usernames, timestamps). Protect these files appropriately.

## Security Updates

Security updates are released as:
- **Patch releases** (e.g., 1.2.1) for minor security fixes
- **Minor releases** (e.g., 1.3.0) for significant security improvements

All security updates are documented in the [CHANGELOG.md](CHANGELOG.md) with a `[SECURITY]` prefix.

## Responsible Disclosure

We follow coordinated vulnerability disclosure:

1. Security researchers report vulnerabilities privately
2. We work to verify and fix the issue
3. We prepare and test a security patch
4. We release the patch publicly
5. We publicly credit the researcher (if desired)

We kindly request that you:
- Give us reasonable time to fix the issue before public disclosure
- Do not exploit the vulnerability
- Do not access, modify, or delete data belonging to others

## Recognition

We appreciate the security research community's efforts. Researchers who responsibly disclose vulnerabilities will be:
- Credited in the security advisory (if desired)
- Mentioned in the CHANGELOG
- Thanked in the project README

---

**Thank you for helping keep MegaDownloadEnhancer and its users safe!**

*Last Updated: 2026-01-20*
