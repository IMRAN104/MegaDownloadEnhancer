# Changelog

All notable changes to Mega Download Enhancer are documented here.

---

## [v1.3.0] — 2026-05-22

### Fixed
- **Stop cycle now works correctly** — the PowerShell VPN cycle script was never actually running due to a wrong parameter name (`-SettingsPath`) being passed; replaced with correct individual parameters matching the script's `param()` block
- **`-UseWarp` boolean binding** — PowerShell's `-File` mode can't coerce string arguments to `[bool]`; changed parameter to `[switch]$UseWarp` so it binds correctly when called from the app
- **UI freeze on Stop** — `StopVpnCycle` now runs on a background thread; previously blocked the UI for up to 15 seconds while killing the process and disconnecting the VPN
- **Double MEGAsync restart on Start** — the app was restarting MEGAsync immediately on Start, then the PS script restarted it again ~15 seconds later; removed the redundant C# restart
- **Double-disconnect on app exit** — `Dispose` was calling `StopVpnCycle` then `Dispose` again internally, causing two VPN disconnect attempts on shutdown
- **WARP false-positive status** — `"Disconnected".Contains("Connected")` returned `true`, making WARP appear connected when it wasn't; fixed to match `"Status update: Connected"`
- **Process stdout deadlock** — `WaitForExit` was called before `ReadToEnd`, which could deadlock if the subprocess wrote more than 4 KB to stdout
- **Thread safety** — `_isRunning` flag marked `volatile` to prevent JIT caching across UI thread and `Process.Exited` ThreadPool callback

### Security
- **Command injection in PowerShell invocations** — user-supplied VPN name was interpolated directly into PowerShell argument strings, allowing injection via embedded quotes/semicolons; fixed by switching to `ProcessStartInfo.ArgumentList` (array form) for the main script launch, and passing VPN name via environment variable (`$env:_VPN_NAME`) for inline status check commands

### Changed
- VPN name and path arguments now use `ArgumentList` instead of string interpolation
- UI auto-updates when the PS cycle exits unexpectedly (wired `StatusChanged` event)
- App exit calls `Dispose` only — no redundant `StopVpnCycle` before dispose

---

## [v1.2.0] — 2026-05-21

### Added
- **Complete UI/UX redesign** — dark theme (`#070B12` base), teal accent (`#00CFA8`), animated status dots, cycle progress display, live clock in footer
- **Custom app icon** — programmatic teal lightning bolt on dark rounded background (GDI+), used in window, tray, and about dialog
- **About dialog** — version, description, GitHub link, rendered entirely via GDI+ canvas
- **Responsive layout** — cards reposition on window resize
- **System tray improvements** — balloon notifications on state changes, context menu with Start/Stop/Settings/Exit
- **GitHub Actions release pipeline** — push a `v*.*.*` tag → automatic self-contained `win-x64` build and GitHub Release

### Fixed
- **Startup crash** — `BeginInvoke cannot be called before window handle created`; moved both `BeginInvoke` calls to the `Load` event handler
- **YAML syntax error** in `release.yml` — PowerShell heredocs at column 0 terminated the YAML block scalar; replaced with array join patterns
- **Dark/light theme** — now follows Windows system preference automatically

### Changed
- Settings form redesigned with sidebar navigation (VPN / General / Appearance)
- README rewritten with compelling pitch targeting the "Transfer quota exceeded" pain point

---

## [v1.0.0] — 2026-05-20

### Added
- Initial release
- WinForms app that cycles any Windows VPN connection on a configurable timer
- Cloudflare WARP support via `warp-cli.exe`
- MEGAsync auto-detect and restart
- `VPN-AutoToggle.ps1` PowerShell script — usable standalone or driven by the GUI
- `Test-VpnSetup.ps1` — 8-check prerequisite validator
- Settings persistence to `%APPDATA%\VPNManager\settings.json`
- DPAPI password encryption for stored VPN credentials
- GitHub Actions CI/CD — build and test on every push

---

[v1.3.0]: https://github.com/IMRAN104/MegaDownloadEnhancer/releases/tag/v1.3.0
[v1.2.0]: https://github.com/IMRAN104/MegaDownloadEnhancer/releases/tag/v1.2.0
[v1.0.0]: https://github.com/IMRAN104/MegaDownloadEnhancer/releases/tag/v1.0.0
