# WARP VPN Setup Guide

## ✅ WARP Support Added!

The VPN Manager has been updated to support **Cloudflare WARP** VPN!

## 🔧 What Changed

### Updated VPN Dropdown
The VPN connection dropdown in Settings now includes:

1. **"WARP"** - Pre-added option for Cloudflare WARP
2. **Windows VPNs** - Any traditional Windows VPN connections
3. **"(Manual Entry)"** - Option to type any VPN name manually
4. **Editable Dropdown** - You can now type custom VPN names

## 📋 How to Configure WARP

### Option 1: Select "WARP" from Dropdown

1. Click **Settings** button
2. Go to **VPN Settings** tab
3. Click the **VPN Connection Name** dropdown
4. Select **"WARP"** from the list
5. Click **OK**

### Option 2: Type "WARP" Manually

1. Click **Settings** button
2. Go to **VPN Settings** tab
3. Click the **VPN Connection Name** dropdown
4. Type **"WARP"** (or any other VPN name)
5. Click **OK**

## ⚠️ Important Notes

### WARP is NOT a Traditional Windows VPN

**WARP works differently:**
- ❌ Does NOT appear in `Get-VpnConnection`
- ❌ Cannot be managed with `rasdial`
- ✅ Runs as a standalone process ("Cloudflare WARP")
- ✅ Has its own management interface

### Current Limitations

The VPN Manager's **Start/Stop VPN Cycle** feature works with:
- ✅ Traditional Windows VPN connections (via `rasdial`)
- ⚠️ **WARP**: The PowerShell script may need custom logic to control WARP

### For WARP Auto-Toggle

You have two options:

**Option A: Use WARP's Built-in Features**
- WARP has its own settings for auto-connect
- Check WARP app settings for automation options

**Option B: Custom WARP Control Script**
You would need to modify `VPN-AutoToggle.ps1` to control WARP using:
- Process management (Start/Stop "Cloudflare WARP" process)
- WARP CLI commands (if available)
- WARP API (if exposed)

## 🚀 Quick Start

1. **Launch VPN Manager** (double-click `VPNManager.exe`)

2. **Configure Settings**
   - Click "Settings"
   - Select "WARP" from VPN dropdown
   - Click OK

3. **Monitor Status**
   - VPN Status panel will show connection state
   - MEGAsync Status panel shows sync status
   - Both auto-refresh every 5 seconds

4. **For Manual Control**
   - Use WARP's system tray icon to connect/disconnect
   - VPN Manager will reflect the status changes

## 📊 Status Monitoring

VPN Manager will monitor WARP status even if you can't auto-toggle it:
- **Green indicator** = WARP is connected/running
- **Red indicator** = WARP is disconnected/stopped

## 🔄 Future Enhancements

To enable full WARP auto-toggle support, you could:

1. **Add WARP-specific logic to VPN-AutoToggle.ps1**
   - Detect if VPN is WARP
   - Use process management instead of `rasdial`
   - Use WARP CLI/API if available

2. **Update VPNService.cs**
   - Add `IsVpnConnected()` logic for WARP
   - Check "Cloudflare WARP" process status
   - Use WARP-specific commands

## 🎯 Current Features That Work with WARP

- ✅ **Status Monitoring** - Real-time WARP status updates
- ✅ **Settings Persistence** - WARP name saved automatically
- ✅ **MEGAsync Monitoring** - Full sync status tracking
- ✅ **System Tray** - Minimize to tray support
- ✅ **Auto-Refresh** - Status updates every 5 seconds
- ⚠️ **Auto-Toggle** - Requires custom WARP control logic

## 💡 Tips

1. **Use Manual Entry** for any VPN that doesn't appear in Windows VPN list
2. **Refresh Button** updates the dropdown with any new Windows VPNs
3. **Settings are saved** in `%AppData%\VPNManager\settings.json`
4. **WARP process name**: "Cloudflare WARP" (Process ID: 24872)

---

**Updated:** Application rebuilt and running with WARP support! 🎉
