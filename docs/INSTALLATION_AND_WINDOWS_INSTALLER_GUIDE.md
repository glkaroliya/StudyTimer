# StudyTimer Installation, Build, and Windows Installer Guide

## 1) Purpose

This guide explains how to:

1. Set up the development environment
2. Build and test StudyTimer
3. Produce distributable binaries
4. Create a Windows installer package professionally

> Repository root used in this guide: `/home/runner/work/StudyTimer/StudyTimer`

---

## 2) Prerequisites

### Required software

- **Windows 10/11** (for installer creation and installer validation)
- **.NET SDK 8.0**
- **Git**
- **Visual Studio 2022** (recommended) with:
  - .NET desktop development workload
  - Optional: MSIX Packaging Tools (if using MSIX flow)

### Verify tooling

```powershell
dotnet --version
git --version
```

---

## 3) Clone and Prepare

```powershell
cd C:\dev
git clone https://github.com/glkaroliya/StudyTimer.git
cd StudyTimer
```

If you are on the hosted runner, use:

```bash
cd /home/runner/work/StudyTimer/StudyTimer
```

---

## 4) Build and Test (Baseline)

Run these commands from the repository root:

```bash
dotnet restore StudyTimer.slnx
dotnet build StudyTimer.slnx
dotnet test StudyTimer.slnx
```

Expected result:

- Build succeeds with 0 errors
- All tests pass

---

## 5) Understand Packaging Scope in This Repository

Current solution contains:

- `StudyTimer.Core` (class library)
- `StudyTimer.Tests` (test project)

A **Windows installer requires an executable app** (for example WPF/WinUI/Console host) that references `StudyTimer.Core`.

So packaging is done in two stages:

1. Build `StudyTimer.Core` (already in this repo)
2. Build/package a Windows host app that depends on `StudyTimer.Core`

---

## 6) Create a Windows Host App (One-Time)

If not already present, create a host application project (example: WPF):

```powershell
cd C:\dev\StudyTimer
dotnet new wpf -n StudyTimer.App -f net8.0-windows
dotnet sln StudyTimer.slnx add .\StudyTimer.App\StudyTimer.App.csproj
dotnet add .\StudyTimer.App\StudyTimer.App.csproj reference .\StudyTimer.Core\StudyTimer.Core.csproj
```

Then implement minimal startup UI that uses services from `StudyTimer.Core`.

---

## 7) Publish Release Binaries

For x64 Windows:

```powershell
cd C:\dev\StudyTimer
dotnet publish .\StudyTimer.App\StudyTimer.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\artifacts\publish\win-x64
```

Artifacts are produced in:

- `artifacts/publish/win-x64`

---

## 8) Create a Professional Windows Installer (WiX v4 Recommended)

## 8.1 Install WiX v4 CLI

```powershell
dotnet tool install --global wix
wix --version
```

## 8.2 Installer project structure

Create directory:

- `installer/`

Create WiX source file:

- `installer/StudyTimer.wxs`

Define MSI metadata professionally:

- Product Name: `StudyTimer`
- Manufacturer: your organization name
- Version: semantic version (e.g., `1.0.0`)
- UpgradeCode: stable GUID (never change after first release)

Install files from:

- `artifacts/publish/win-x64`

Include:

- Start Menu shortcut
- Desktop shortcut (optional)
- Add/Remove Programs entry
- Proper upgrade behavior (major upgrades)

## 8.3 Build MSI

```powershell
cd C:\dev\StudyTimer
wix build .\installer\StudyTimer.wxs -arch x64 -o .\artifacts\installer\StudyTimer-x64.msi
```

Output:

- `artifacts/installer/StudyTimer-x64.msi`

---

## 9) Optional: MSIX Packaging Path

If your organization requires Store-style deployment:

- Create an MSIX Packaging Project in Visual Studio
- Reference `StudyTimer.App`
- Configure identity, publisher, versioning, capabilities
- Build signed `.msix` package

Choose MSIX when you need:

- Enterprise deployment tooling
- Cleaner uninstall/upgrade behavior
- Sandboxed distribution policy

---

## 10) Code Signing (Production Required)

Before external distribution:

1. Obtain a trusted code-signing certificate
2. Sign the installer (`.msi` or `.msix`)
3. Timestamp the signature

Unsigned installers reduce trust and can trigger SmartScreen warnings.

---

## 11) Installer Validation Checklist

Validate on a clean Windows VM:

- Fresh install succeeds
- App launches from Start Menu
- Uninstall removes files and shortcuts
- Reinstall/upgrade from previous version works
- Application data migration behavior verified

---

## 12) CI/CD Packaging Recommendation

Automate release packaging in CI:

1. Restore/build/test
2. Publish host app binaries
3. Build installer
4. Sign installer (secure secret storage)
5. Upload release artifacts

Suggested release artifacts:

- `StudyTimer-x64.msi`
- checksums (`SHA256SUMS.txt`)
- release notes

---

## 13) Troubleshooting

### Build succeeds but no installer possible

Cause: no executable host project yet.  
Fix: create `StudyTimer.App` and reference `StudyTimer.Core`.

### Installer builds but app fails to start

Cause: missing runtime dependencies or wrong publish mode.  
Fix: use self-contained publish and verify required native dependencies.

### Upgrade installs side-by-side

Cause: incorrect MSI upgrade metadata.  
Fix: keep stable `UpgradeCode`, increment version correctly, configure major upgrade behavior.

---

## 14) Minimum Professional Release Standard

Before shipping:

- All tests pass on release commit
- Installer is signed
- Installer validated on clean machine
- Upgrade path tested from previous version
- User guide is updated for released version
