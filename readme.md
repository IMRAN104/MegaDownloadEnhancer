# Mega Download Enhancer

[![Build Status](https://github.com/IMRAN104/MegaDownloadEnhancer/actions/workflows/build-test.yml/badge.svg)](https://github.com/IMRAN104/MegaDownloadEnhancer/actions/workflows/build-test.yml)
[![Latest Release](https://img.shields.io/github/v/release/IMRAN104/MegaDownloadEnhancer)](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/IMRAN104/MegaDownloadEnhancer/total)](https://github.com/IMRAN104/MegaDownloadEnhancer/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue)](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)

> **Download from MEGA at full speed — forever. No premium required.**

---

## The Problem

MEGA throttles free users. Hit the daily bandwidth limit and you're staring at this:

```
Transfer quota exceeded. Your download will resume in 6 hours.
```

Six hours. For a file you need *now*.

## The Solution

**Mega Download Enhancer** automatically cycles your IP address on a timer using any Windows VPN — Cloudflare WARP, OpenVPN, WireGuard, or any connection you already have — then restarts MEGAsync so it picks up the fresh IP. MEGA sees a new user. Downloads resume instantly.

Set it once. Walk away. It runs silently in your system tray.

---

## 📥 Download — No Installation Required

**[⬇ Download Latest Release](https://github.com/IMRAN104/MegaDownloadEnhancer/releases/latest)**

1. Download `VPNManager-vX.X.X-win-x64.zip` (latest version shown on release page)
2. Extract anywhere
3. Run `VPNManager.exe`
4. Done

No .NET install needed. Self-contained, single folder, no registry mess.

---

## How It Works

```
Every N minutes (default: 6):
  1. Disconnect VPN   →  MEGA sees your real IP
  2. Reconnect VPN    →  New IP assigned
  3. Restart MEGAsync →  Fresh session, quota reset
  4. Downloads resume at full speed
```

The cycle runs in the background. You get a system tray icon, balloon notifications on state changes, and a live status window if you want it.

---

## Features

| Feature | Details |
|---------|---------|
| **Any Windows VPN** | Works with Cloudflare WARP, OpenVPN, WireGuard, or any VPN in Windows Settings |
| **MEGAsync companion** | Auto-detects and restarts the sync client |
| **System tray** | Runs silently, always accessible |
| **Dark / Light theme** | Follows Windows system preference |
| **Auto-start with Windows** | Toggle from the tray menu |
| **Configurable timing** | 1–60 minute cycles, your call |

---

## Requirements

- Windows 10 or 11
- **A VPN that changes your IP on reconnect** (required — this is the core mechanism):
  - **[Cloudflare WARP](https://1.1.1.1/)** — free, recommended, reconnects in ~2 seconds
  - Any VPN connection in Windows Settings → Network → VPN (OpenVPN, WireGuard, commercial VPNs, etc.)
- MEGAsync desktop client

No proxy. No browser extension. A VPN is the only external dependency.

---

## Quick Setup (2 minutes)

### Step 1 — Have a VPN ready
Any VPN that changes your IP on reconnect works. Don't have one? [Cloudflare WARP](https://1.1.1.1/) is free, fast, and takes 30 seconds to install. Connect it once manually to confirm it works.

### Step 2 — Run the app
Extract the zip, launch `VPNManager.exe`. First-time setup opens automatically.

### Step 3 — Configure
- Select your VPN from the dropdown (WARP users: select `CloudflareWARP`)
- Set cycle duration (start with 6 minutes — matches MEGA's quota window)
- Enable MEGAsync monitoring
- Click **Start Cycle**

The app minimizes to tray. Downloads continue uninterrupted.

---

---

## Advanced: PowerShell Script

Power users can use the included `VPN-AutoToggle.ps1` directly:

```powershell
# Basic — uses CloudflareWARP, 10-minute cycles
.\VPN-AutoToggle.ps1

# Custom VPN and timing
.\VPN-AutoToggle.ps1 -VpnName "MyVPN" -CycleDurationMinutes 6

# All options
.\VPN-AutoToggle.ps1 `
    -VpnName "CloudflareWARP" `
    -CycleDurationMinutes 6 `
    -LogPath "C:\Logs\mega.log" `
    -MaxRetries 3
```

Validate your setup first:
```powershell
.\Test-VpnSetup.ps1 -VpnName "CloudflareWARP"
```

---

## FAQ

**Does this work with the MEGA browser extension?**
This tool targets MEGAsync (the desktop sync client). For browser downloads, use it alongside any VPN that rotates IP on reconnect.

**Is Cloudflare WARP actually free?**
Yes. The base WARP service is completely free, assigns you a new IP on each connect, and has no data cap.

**Will MEGA ban my account?**
This tool bypasses bandwidth throttling, not account restrictions. It does not automate paid-feature access or violate content policies.

**Can I use a different VPN?**
Yes — any VPN connection visible in Windows VPN settings works. WARP is recommended because it's free and reconnects in under 2 seconds.

**Does it run when I close the window?**
Yes. Closing the window minimizes to tray. Use *Exit* from the tray menu to fully quit.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| VPN not in dropdown | Add it in Windows Settings → Network & Internet → VPN first |
| WARP not found | Run `warp-cli.exe status` in terminal to verify WARP is installed |
| MEGAsync not detected | Open MEGAsync first, let it fully load |
| Cycle starts but downloads don't resume | Increase cycle duration to 8–10 min |
| VPN toggle fails | Run as Administrator |

---

## Building from Source

```powershell
dotnet publish VPNManager/VPNManager.csproj -c Release -r win-x64 --self-contained true
```

Or use GitHub Actions — push a `v*.*.*` tag and it builds and releases automatically.

---

## License

MIT. Use it, fork it, improve it.

## Contributing

Issues and PRs welcome. If this saved you hours of waiting, a ⭐ star costs nothing.

---

**Made for anyone who's ever watched a MEGA countdown timer and thought "there has to be a better way."**
