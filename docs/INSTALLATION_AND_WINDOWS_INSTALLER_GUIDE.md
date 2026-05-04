# StudyTimer Installation, Build, and Windows Installer Guide

## 1) Purpose

This guide explains how to:

1. Set up the development environment
2. Build and test StudyTimer
3. Run the WPF desktop app locally
4. Publish distributable binaries
5. Create a Windows installer package

> Repository root used in this guide: `/home/runner/work/StudyTimer/StudyTimer`

---

## 2) Current Solution Layout

The solution currently contains:

- `StudyTimer.Core` - production domain/services library
- `StudyTimer.Tests` - xUnit test project
- `StudyTimer.App` - WPF desktop executable (`net10.0-windows`)
- `StudyTimer.slnx` - solution file

---

## 3) Prerequisites

### Required software

- **.NET SDK 10.0**
- **Git**
- **Windows 10/11** (for running the WPF app and creating installers)
- **Visual Studio 2022** (recommended) with:
  - .NET desktop development workload
  - Optional: MSIX Packaging Tools (if using MSIX flow)

### Verify tooling

```powershell
dotnet --version
git --version
```

---

## 4) Clone and Prepare

```powershell
cd C:\dev
git clone https://github.com/glkaroliya/StudyTimer.git
cd StudyTimer
```

If you are on the hosted runner:

```bash
cd /home/runner/work/StudyTimer/StudyTimer
```

---

## 5) Restore, Build, and Test

Run from repository root:

```bash
dotnet restore StudyTimer.slnx
dotnet build StudyTimer.slnx
dotnet test StudyTimer.slnx
```

Expected result:

- Build succeeds
- All tests pass

> Note: `StudyTimer.App` targets `net10.0-windows` and includes `EnableWindowsTargeting=true` for compatibility with non-Windows build environments (such as Linux CI).

---

## 6) Run the Desktop App Locally

Run the WPF app:

```bash
dotnet run --project StudyTimer.App/StudyTimer.App.csproj
```

### First login

On first run, a default admin user is seeded automatically if no users exist:

- Username: `admin`
- Password: `Admin123`

You can then sign in and use the full UI (Dashboard, Timetable, Timer, Progress, Settings, etc.).

---

## 7) Publish Release Binaries

For Windows x64 self-contained publish:

```powershell
cd C:\dev\StudyTimer
dotnet publish .\StudyTimer.App\StudyTimer.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\artifacts\publish\win-x64
```

Publish output:

- `artifacts/publish/win-x64`

---

## 8) Create a Windows Installer (WiX v4 Recommended)

### 8.1 Install WiX CLI

```powershell
dotnet tool install --global wix
wix --version
```

### 8.2 Installer project structure

Create an installer directory and WiX source file:

- `installer/`
- `installer/StudyTimer.wxs`

Use published files from:

- `artifacts/publish/win-x64`

Recommended installer metadata:

- Product Name: `StudyTimer`
- Manufacturer: your organization name
- Version: semantic version (for example `1.0.0`)
- UpgradeCode: stable GUID (keep constant after first release)

Recommended installer behavior:

- Start Menu shortcut
- Desktop shortcut (optional)
- Add/Remove Programs registration
- Proper major upgrade handling

### 8.3 Build MSI

```powershell
cd C:\dev\StudyTimer
wix build .\installer\StudyTimer.wxs -arch x64 -o .\artifacts\installer\StudyTimer-x64.msi
```

Output:

- `artifacts/installer/StudyTimer-x64.msi`

---

## 9) Optional: MSIX Packaging Path

If your organization requires Store/enterprise style deployment:

- Create an MSIX Packaging Project in Visual Studio
- Reference `StudyTimer.App`
- Configure identity, publisher, version, and capabilities
- Build a signed `.msix` package

---

## 10) Code Signing (Production Required)

Before external distribution:

1. Obtain a trusted code-signing certificate
2. Sign the installer (`.msi` or `.msix`)
3. Timestamp the signature

Unsigned installers may trigger SmartScreen warnings.

---

## 11) Installer Validation Checklist

Validate on a clean Windows VM:

- Fresh install succeeds
- App launches from Start Menu shortcut
- Login screen opens successfully
- Default admin sign-in works on first run
- Uninstall removes files and shortcuts
- Reinstall/upgrade from previous version works

---

## 12) CI/CD Packaging Recommendation

Automate release packaging in CI:

1. Restore/build/test solution
2. Publish `StudyTimer.App`
3. Build installer (WiX/MSIX)
4. Sign installer (using secure secret/certificate handling)
5. Upload release artifacts

Suggested release artifacts:

- `StudyTimer-x64.msi`
- `SHA256SUMS.txt`
- Release notes

---

## 13) Troubleshooting

### Build fails for `StudyTimer.App` on non-Windows machine

- Ensure .NET SDK 10 is installed
- Ensure the project still has `EnableWindowsTargeting=true`
- Run `dotnet restore` before `dotnet build`

### App starts but login fails

- Confirm credentials
- On first run, use `admin` / `Admin123`
- If repeated failed attempts occurred, wait for lockout window to expire

### Installer builds but app fails to launch

- Verify publish step used the intended runtime and output path
- Rebuild publish artifacts, then rebuild installer

### Upgrade installs side-by-side

- Verify MSI upgrade metadata
- Keep stable `UpgradeCode`
- Increment product version correctly and configure major upgrade behavior

---

## 14) Minimum Professional Release Standard

Before shipping:

- Build and tests pass on release commit
- Installer is signed
- Installer validated on clean machine
- Upgrade path tested from previous version
- User guide updated for released version
