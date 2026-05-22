# Graph Report - .  (2026-05-22)

## Corpus Check
- Corpus is ~30,176 words - fits in a single context window. You may not need a graph.

## Summary
- 228 nodes · 358 edges · 20 communities (11 shown, 9 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 17 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_MainForm UI Components|MainForm UI Components]]
- [[_COMMUNITY_VPN Toggle Script & Docs|VPN Toggle Script & Docs]]
- [[_COMMUNITY_WARPCI Config & Guides|WARP/CI Config & Guides]]
- [[_COMMUNITY_Settings UI Components|Settings UI Components]]
- [[_COMMUNITY_VpnService Core Logic|VpnService Core Logic]]
- [[_COMMUNITY_About Dialog & Theming|About Dialog & Theming]]
- [[_COMMUNITY_AppSettings & MegaService|AppSettings & MegaService]]
- [[_COMMUNITY_Theme Utilities|Theme Utilities]]
- [[_COMMUNITY_GitHub Actions CICD|GitHub Actions CI/CD]]
- [[_COMMUNITY_Settings Model|Settings Model]]
- [[_COMMUNITY_Program Entry Point|Program Entry Point]]
- [[_COMMUNITY_Test-VpnSetup Script|Test-VpnSetup Script]]
- [[_COMMUNITY_VpnStatus Model|VpnStatus Model]]
- [[_COMMUNITY_MegaStatus Model|MegaStatus Model]]
- [[_COMMUNITY_Security & Auth Policy|Security & Auth Policy]]
- [[_COMMUNITY_Windows Setup Guides|Windows Setup Guides]]
- [[_COMMUNITY_Auto-Restart Script|Auto-Restart Script]]
- [[_COMMUNITY_GitHub Release Guide|GitHub Release Guide]]
- [[_COMMUNITY_Release Build Script|Release Build Script]]

## God Nodes (most connected - your core abstractions)
1. `MainForm` - 48 edges
2. `SettingsForm` - 29 edges
3. `VpnService` - 18 edges
4. `VPN-AutoToggle.ps1 Main Script` - 13 edges
5. `MegaService` - 9 edges
6. `Write-Log()` - 7 edges
7. `Start-Main()` - 7 edges
8. `Mega Download Enhancer Project` - 6 edges
9. `Connect-VpnWithRetry()` - 5 edges
10. `AboutDialog` - 5 edges

## Surprising Connections (you probably didn't know these)
- `Semantic Versioning` --conceptually_related_to--> `Release Workflow (release.yml)`  [INFERRED]
  CHANGELOG.md → CI_CD_GUIDE.md
- `Graceful Shutdown via Ctrl+C` --semantically_similar_to--> `Stop-Script Graceful Shutdown Function`  [INFERRED] [semantically similar]
  SETUP-AND-USAGE.md → Claude.md
- `MEGA Bandwidth Throttling Bypass` --semantically_similar_to--> `IP Rotation Use Case`  [INFERRED] [semantically similar]
  readme.md → Claude.md
- `Workflow Validation Checklist` --references--> `Release Workflow (release.yml)`  [EXTRACTED]
  .github/WORKFLOW_VALIDATION.md → CI_CD_GUIDE.md
- `SHA256 Checksum Verification` --references--> `Release Package (VPNManager-win-x64.zip)`  [INFERRED]
  SECURITY.md → CI_CD_GUIDE.md

## Hyperedges (group relationships)
- **VPN Auto-Toggle Core Cycle (Script + Connect + Disconnect)** — claudemd_vpn_autotoggle_script, claudemd_connect_vpnwithretry_fn, claudemd_disconnect_vpnwithretry_fn, claudemd_rasdial [EXTRACTED 1.00]
- **WinForms App Architecture (MainForm + VpnService + PowerShell)** — pr_desc_mainform, pr_desc_vpnservice, claudemd_vpn_autotoggle_script, pr_desc_json_settings_bridge [EXTRACTED 1.00]
- **CI/CD Release Pipeline (GitHub Actions + Build + Release Package)** — cicd_github_actions, cicd_build_test_workflow, cicd_release_workflow, cicd_release_package [EXTRACTED 1.00]
- **WARP VPN Auto-Toggle Full Cycle** — vpnservice_cs, vpn_autotoggle_ps1, vpn_settings_json [EXTRACTED 0.95]
- **GitHub Actions CI/CD Release Pipeline** — build_test_workflow, release_workflow, vpnmanager_csproj [EXTRACTED 0.95]
- **MEGAsync Restart on VPN Toggle Flow** — mainform_cs, vpnservice_cs, vpn_autotoggle_ps1 [INFERRED 0.85]

## Communities (20 total, 9 thin omitted)

### Community 0 - "MainForm UI Components"
Cohesion: 0.08
Nodes (12): Button, ContextMenuStrip, DateTime, MainForm, VPNManager.Forms, Label, MegaService, NotifyIcon (+4 more)

### Community 1 - "VPN Toggle Script & Docs"
Cohesion: 0.06
Nodes (39): Semantic Versioning, Version 1.0.0 Initial Release, VPN Auto-Toggle Project, Release Package (VPNManager-win-x64.zip), Connect-VpnWithRetry Function, Disconnect-VpnWithRetry Function, Get-VpnConnection PowerShell Cmdlet, IP Rotation Use Case (+31 more)

### Community 2 - "WARP/CI Config & Guides"
Cohesion: 0.13
Nodes (29): AppData settings.json Persistent Settings, Build Success Documentation, Build and Test GitHub Actions Workflow, Cloudflare WARP Integration Guide, Self-Contained .NET Build Strategy, Final Fixes Implementation Summary, VPN Manager Installation Guide, Connect-VpnWithRetry() (+21 more)

### Community 3 - "Settings UI Components"
Cohesion: 0.19
Nodes (6): CheckBox, ComboBox, SettingsForm, VPNManager.Forms, NumericUpDown, TextBox

### Community 4 - "VpnService Core Logic"
Cohesion: 0.16
Nodes (5): bool, IDisposable, Process, VPNManager.Services, VpnService

### Community 5 - "About Dialog & Theming"
Cohesion: 0.20
Nodes (6): Color, Form, AboutDialog, VPNManager.Forms, AppIcon, VPNManager.Forms

### Community 6 - "AppSettings & MegaService"
Cohesion: 0.27
Nodes (3): AppSettings, MegaService, VPNManager.Services

### Community 8 - "GitHub Actions CI/CD"
Cohesion: 0.40
Nodes (6): Build and Test Workflow (build-test.yml), .NET 10.0 SDK Build Target, GitHub Actions CI/CD, Release Workflow (release.yml), Workflow Validation Checklist, win-x64 Self-Contained Build Config

### Community 9 - "Settings Model"
Cohesion: 0.33
Nodes (3): AppSettings, VPNManager.Models, string

### Community 10 - "Program Entry Point"
Cohesion: 0.40
Nodes (3): Mutex, Program, VPNManager

## Knowledge Gaps
- **37 isolated node(s):** `VPNManager`, `Mutex`, `VPNManager.Forms`, `VPNManager.Forms`, `VPNManager.Forms` (+32 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **9 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MainForm` connect `MainForm UI Components` to `VpnService Core Logic`, `About Dialog & Theming`, `AppSettings & MegaService`?**
  _High betweenness centrality (0.166) - this node is a cross-community bridge._
- **Why does `SettingsForm` connect `Settings UI Components` to `MainForm UI Components`, `Settings Model`, `About Dialog & Theming`, `AppSettings & MegaService`?**
  _High betweenness centrality (0.119) - this node is a cross-community bridge._
- **Why does `AppSettings` connect `AppSettings & MegaService` to `MainForm UI Components`, `Settings UI Components`, `VpnService Core Logic`?**
  _High betweenness centrality (0.094) - this node is a cross-community bridge._
- **What connects `VPNManager`, `Mutex`, `VPNManager.Forms` to the rest of the system?**
  _47 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MainForm UI Components` be split into smaller, more focused modules?**
  _Cohesion score 0.0821256038647343 - nodes in this community are weakly interconnected._
- **Should `VPN Toggle Script & Docs` be split into smaller, more focused modules?**
  _Cohesion score 0.0620782726045884 - nodes in this community are weakly interconnected._
- **Should `WARP/CI Config & Guides` be split into smaller, more focused modules?**
  _Cohesion score 0.13257575757575757 - nodes in this community are weakly interconnected._