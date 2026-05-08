# Apps Launcher

**Apps Launcher** is a lightweight Windows desktop utility that gives you a single, tidy panel to open, monitor, and close all your frequently used applications, scripts, and tools — with one click. No more hunting through the Start menu or desktop shortcuts. Everything lives in one configurable table.

It also ships with a companion **Clipper** window: a one-click clipboard manager for tokens, passwords, keys, and any text snippets you paste repeatedly.

> **You do not need to be a developer to use this app.** Once it is running, everything is point-and-click.

---

---

## Table of Contents

1. [What Does This App Do?](#1-what-does-this-app-do)
2. [Prerequisites — What You Need Before Starting](#2-prerequisites--what-you-need-before-starting)
3. [How to Run the Application](#3-how-to-run-the-application)
   - [Option A — Run from Source Code (Developer)](#option-a--run-from-source-code-developer)
   - [Option B — Build a Standalone Executable (Recommended for Daily Use)](#option-b--build-a-standalone-executable-recommended-for-daily-use)
4. [First Launch — Getting Oriented](#4-first-launch--getting-oriented)
5. [Apps Launcher Window — Complete Guide](#5-apps-launcher-window--complete-guide)
   - [The Toolbar (Top Bar of Buttons)](#the-toolbar-top-bar-of-buttons)
   - [The Table Columns](#the-table-columns)
   - [Status Badge — What Each Colour Means](#status-badge--what-each-colour-means)
   - [Auto Status Polling (Live Monitoring)](#auto-status-polling-live-monitoring)
   - [Edit Lock — Protecting Your Configuration](#edit-lock--protecting-your-configuration)
6. [Step-by-Step: How to Use the App](#6-step-by-step-how-to-use-the-app)
   - [Adding a New Application or Script](#adding-a-new-application-or-script)
   - [Launching an Application](#launching-an-application)
   - [Killing / Stopping an Application](#killing--stopping-an-application)
   - [Removing a Row](#removing-a-row)
   - [Saving Your Configuration](#saving-your-configuration)
7. [Clipper Window — Clipboard Manager](#7-clipper-window--clipboard-manager)
   - [What is Clipper?](#what-is-clipper)
   - [How to Use Clipper](#how-to-use-clipper)
8. [TaskKill Command — Explained Simply](#8-taskkill-command--explained-simply)
9. [Supported File Types](#9-supported-file-types)
10. [Configuration File — launcher-config.json](#10-configuration-file--launcher-configjson)
    - [Where the File Lives](#where-the-file-lives)
    - [File Structure](#file-structure)
    - [Save Behaviour and Backups](#save-behaviour-and-backups)
    - [What Gets Remembered](#what-gets-remembered)
11. [Help Button](#11-help-button)
12. [Tips and Tricks](#12-tips-and-tricks)
13. [Troubleshooting](#13-troubleshooting)
14. [Project Structure (For Developers)](#14-project-structure-for-developers)
15. [Build Commands Reference (For Developers)](#15-build-commands-reference-for-developers)

---

## 1. What Does This App Do?

Apps Launcher solves a simple but common problem: **you have many apps, scripts, and tools you open regularly, and clicking through the Start menu or hunting for shortcuts wastes time**.

With Apps Launcher you:

- Keep a personal list of everything you want to be able to open — programs, batch files, PowerShell scripts, Remote Desktop connections, Python scripts, VBS scripts, and more.
- **Launch any of them with one click** from a single tidy table.
- **See live status** — the app tells you whether each program is currently running or not, and automatically refreshes every 5 seconds.
- **Close / force-stop programs** with one click (using Windows `taskkill` behind the scenes).
- **Clipper companion window** — keep a list of text files (API tokens, passwords, SSH keys, snippets) and copy any of them to the clipboard with one click.
- Everything is **saved automatically** — your list persists across restarts.

---

## 2. Prerequisites — What You Need Before Starting

### For daily use (running a pre-built `.exe`)

| Requirement | Where to get it | Notes |
|---|---|---|
| **Windows 10 or Windows 11** | Already on your PC | Older Windows versions are not supported |
| **.NET 8 Windows Desktop Runtime** | [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — choose *".NET Desktop Runtime 8.x"* | Only needed if using a framework-dependent build. If you use a self-contained build, skip this. |

> **How to check if .NET 8 is already installed:**
> Open PowerShell or Command Prompt and type:
> ```
> dotnet --list-runtimes
> ```
> If you see a line containing `Microsoft.WindowsDesktop.App 8.`, you are good to go.

### For building / running from source code (Developer)

| Requirement | Where to get it |
|---|---|
| **Windows 10 or Windows 11** | Already on your PC |
| **.NET 8 SDK** | [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — choose *".NET SDK 8.x"* |
| **Git** (optional, for cloning) | [https://git-scm.com](https://git-scm.com) |

---

## 3. How to Run the Application

### Option A — Run from Source Code (Developer)

1. Open a **PowerShell** or **Command Prompt** window.
2. Navigate to the project folder:
   ```powershell
   cd "C:\path\to\AppsLauncher"
   ```
3. Run the app:
   ```powershell
   dotnet run
   ```
   The window will appear. Your config file will be created at:
   `bin\Debug\net8.0-windows\launcher-config.json`

### Option B — Build a Standalone Executable (Recommended for Daily Use)

This creates a single `.exe` file you can double-click like any normal Windows application.

**Self-contained build** — no .NET installation needed on the target machine:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
The output `.exe` will be in:
`bin\Release\net8.0-windows\win-x64\publish\AppsLauncher.exe`

Copy that file (and its `launcher-config.json` if you have one) anywhere you like and double-click it.

**Framework-dependent build** — smaller file, but requires .NET 8 Desktop Runtime installed:
```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

> **Tip:** Create a shortcut to `AppsLauncher.exe` and pin it to your taskbar for instant access every day.

---

## 4. First Launch — Getting Oriented

When you open Apps Launcher for the first time, you will see:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ 🚀  Apps Launcher    Configure and launch your apps, scripts, and tools  │
├────────────────────────────────────────────────────────────────────────── │
│ [+ Add Row]  [− Remove Row]  |  [💾 Save]  |  [🔒 Editing: OFF]  [📋 Clipper]  [? Help]  │
├──────────────────┬──────────────────────────────┬──────┬────────┬──────────────────────┤
│  Application     │  Program / Script            │      │ Status │  TaskKill Command    │
│  (bold name)     │  (path to file)              │  ▶   │ badge  │  + Kill button       │
└──────────────────┴──────────────────────────────┴──────┴────────┴──────────────────────┘
```

The table is empty until you add rows. The status bar at the very bottom shows messages like `Ready` or `Loaded 7 item(s).`

---

## 5. Apps Launcher Window — Complete Guide

### The Toolbar (Top Bar of Buttons)

| Button | What it Does |
|---|---|
| **+ Add Row** | Adds a blank new row at the bottom of the table. Fill in the Application name and Program/Script path. |
| **− Remove Row** | Deletes whichever row is currently highlighted (click a row first to select it). |
| **💾 Save** | Saves your configuration to disk immediately and creates a timestamped backup of the previous version. |
| **🔒 Editing: OFF** / **🔓 Editing: ON** | Toggles edit protection. When OFF (locked), the Application name and Program/Script path cells cannot be accidentally changed. Click to turn editing ON when you want to make changes, then lock again when done. |
| **📋 Clipper** | Opens the Clipper companion window (see [Section 7](#7-clipper-window--clipboard-manager)). Clicking again brings it to the front. |
| **? Help** | Opens this README file in your default text or Markdown viewer. |

### The Table Columns

| Column | What it Shows | Can You Edit It? |
|---|---|---|
| **Application** | The display name you give to this entry (e.g. `MS-Teams`, `My Database Script`). Shown in **bold**. | Yes — only when Editing is ON |
| **Program / Script** | The full file path to the application or script (e.g. `C:\Program Files\...\teams.exe`). | Yes — only when Editing is ON |
| *(Launch button)* | A green **▶ Launch** button — click it to start the application. | N/A |
| **Status** | A colour-coded badge showing whether the application is currently running. Updates automatically every 5 seconds. | No — read-only, updates automatically |
| **TaskKill Command** | The command used to force-stop this application (e.g. `taskkill /F /IM ms-teams.exe`). Shown as text; the black **Kill** button is next to it. | Edit directly in `launcher-config.json` (see [Section 10](#10-configuration-file--launcher-configjson)) |

> **Why is TaskKill read-only in the UI?**
> The TaskKill command is a system-level instruction. Editing it carelessly could stop the wrong process. It is intentionally protected — you set it once in the config file and it just works.

### Status Badge — What Each Colour Means

| Badge | Colour | Meaning |
|---|---|---|
| `— Idle` | Grey | Not launched yet this session |
| `● Running` | Green | The process is currently running on your PC |
| `◼ Stopped` | Orange | Was running but has since exited or crashed |
| `■ Terminated` | Red | You clicked Kill and it was stopped successfully |
| `⚠ Error` | Amber | Something went wrong during launch or kill |

> **Important:** Status is never saved to the config file. Every time you restart Apps Launcher, all badges reset to `— Idle`. This is by design — the app checks live at startup.

### Auto Status Polling (Live Monitoring)

For any row that has a **TaskKill Command** set, Apps Launcher automatically checks every **5 seconds** whether that process is currently running on your PC. You do not need to click anything — the badges update on their own.

Rows with no TaskKill Command are skipped in polling (since there is no process name to check).

### Edit Lock — Protecting Your Configuration

The **🔒 Editing: OFF** button protects you from accidentally changing application names or file paths.

- **🔒 OFF (locked, amber button):** Application and Program/Script cells are read-only. Clicking them does nothing. This is the default every time you start the app.
- **🔓 ON (unlocked, green button):** Application and Program/Script cells become editable. Double-click any cell to change it.

After making your changes, click the button again to lock and then click **💾 Save**.

---

## 6. Step-by-Step: How to Use the App

### Adding a New Application or Script

1. Click **+ Add Row** — a blank row appears at the bottom.
2. Click **🔓 Editing: ON** to unlock editing (if it is not already unlocked).
3. **Double-click the Application cell** (first column) and type a friendly name, e.g. `MS-Teams`.
4. **Double-click the Program/Script cell** (second column) and paste the full path to the file, e.g.:
   ```
   C:\Program Files\WindowsApps\MSTeams_...\ms-teams.exe
   ```
   > **Tip:** In Windows Explorer, hold **Shift** and right-click the file → *Copy as path* to get the exact full path.
5. *(Optional)* If you want to be able to kill this app with the Kill button, open `launcher-config.json` in Notepad and add a `TaskKillCommand` for this item (see [Section 8](#8-taskkill-command--explained-simply)).
6. Click **💾 Save** to persist your changes.

### Launching an Application

1. Find the row for the application you want to open.
2. Click the green **▶ Launch** button in that row.
3. The application opens. The Status badge will update to `● Running` within a few seconds (if a TaskKill Command is set).

### Killing / Stopping an Application

1. Find the row for the running application.
2. Click the black **⬛ Kill** button at the right of that row.
3. The app sends a `taskkill` command to Windows. The Status badge updates to `■ Terminated`.

> The Kill button only works if that row has a **TaskKill Command** set in the config file.

### Removing a Row

1. Click the row you want to delete to **select it** (it will highlight blue).
2. Click **− Remove Row**.
3. The row is removed immediately from the table.
4. Click **💾 Save** to make the deletion permanent.

### Saving Your Configuration

- Click **💾 Save** at any time to write your current table to disk.
- A **timestamped backup** of the previous config is automatically created every time you press Save (e.g. `launcher-config_20260508T143000.json`).
- The config is also **saved silently when you close the window** (no backup on auto-save at close).

---

## 7. Clipper Window — Clipboard Manager

### What is Clipper?

Clipper is a companion panel that holds a list of **plain text files** you frequently paste. Instead of opening a file, selecting all, and copying — you click one button and the text is instantly on your clipboard.

Common uses:
- **API tokens / PAT tokens** — paste into a browser or terminal in one click
- **Passwords or keys** — stored locally in a plain file, copied when needed
- **Command-line snippets** — long commands you run regularly
- **Network drive paths, email addresses, server names** — anything you paste repeatedly

### How to Use Clipper

1. Click **📋 Clipper** in the main toolbar.
2. The Clipper window opens alongside Apps Launcher.
3. To **add a new snippet**:
   - Click **+ Add Row**.
   - Double-click the **Label** cell and give it a name (e.g. `GitHub Token`).
   - Double-click the **Source File** cell and paste the full path to your text file (e.g. `C:\...\Resources\Clipper-GitHub-PAT-Token`).
   - Click **💾 Save**.
4. To **copy a snippet**: Click the teal **📋 Copy** button. The file's contents are instantly placed on your clipboard. The status bar shows how many characters were copied.

> **File format:** The source file must be a plain text file. It does not need a file extension. Create it with Notepad, VS Code, or any text editor and save it with just the token/key/snippet inside.

---

## 8. TaskKill Command — Explained Simply

Windows has a built-in command called `taskkill` that can force-stop a running program. Apps Launcher uses this command when you click the **Kill** button.

You need to know the **process name** of the application — this is usually the name of its `.exe` file.

> **How to find the process name:** Open Task Manager (Ctrl+Shift+Esc) → Details tab → look for the `.exe` name of the running application.

All three of these formats work in the TaskKill Command field:

| What you type | What it means |
|---|---|
| `ms-teams.exe` | Just the exe name — Apps Launcher auto-adds `taskkill /F /IM` |
| `/IM ms-teams.exe /F` | Partial command — Apps Launcher prepends `taskkill` |
| `taskkill /F /IM ms-teams.exe` | Full command — used exactly as written |

**Examples:**

| Application | TaskKill Command to use |
|---|---|
| Microsoft Teams | `taskkill /F /IM ms-teams.exe` |
| Microsoft Outlook | `taskkill /F /IM OUTLOOK.EXE` |
| Microsoft OneNote | `taskkill /F /IM ONENOTE.EXE` |
| Notepad | `taskkill /F /IM notepad.exe` |
| Windows Script Host (`.vbs`) | `taskkill /F /IM WScript.exe` |

> **Note:** The TaskKill Command field in the table is **display-only**. To set or change it, open `launcher-config.json` in Notepad and edit the `"TaskKillCommand"` value for the relevant item. See [Section 10](#10-configuration-file--launcher-configjson).

---

## 9. Supported File Types

Apps Launcher can open virtually any file that Windows knows how to handle.

| File Type | Extension(s) | How it Opens |
|---|---|---|
| Windows executable | `.exe` | Runs directly |
| Batch script | `.bat`, `.cmd` | Runs in Windows command processor |
| PowerShell script | `.ps1` | Runs via `powershell.exe -ExecutionPolicy Bypass -File` |
| Python script | `.py` | Runs via `python` (Python must be installed and on your PATH) |
| Remote Desktop | `.rdp` | Opens Remote Desktop Connection |
| VBScript | `.vbs` | Runs via Windows Script Host |
| Any other file | `*` | Opens with the default Windows handler (same as double-clicking in Explorer) |

---

## 10. Configuration File — launcher-config.json

All your settings — application list, Clipper list, window sizes, and column widths — are stored in a single human-readable file called `launcher-config.json`.

### Where the File Lives

| How you run the app | Config file location |
|---|---|
| `dotnet run` (developer mode) | `bin\Debug\net8.0-windows\launcher-config.json` |
| Published / standalone `.exe` | Same folder as `AppsLauncher.exe` |

### File Structure

The file is formatted JSON — readable in any text editor like Notepad, VS Code, or Notepad++.

```json
{
  "Launcher": {
    "WindowWidth": 900,
    "WindowHeight": 480,
    "ColumnWidths": {
      "Application": 160,
      "ProgramScript": 289,
      "Launch": 95,
      "Status": 82,
      "TaskKill": 258
    },
    "Items": [
      {
        "Label": "MS-Teams",
        "FilePath": "C:\\Program Files\\...\\ms-teams.exe",
        "Parameters": "",
        "TaskKillCommand": "taskkill /F /IM ms-teams.exe"
      },
      {
        "Label": "My Script",
        "FilePath": "C:\\Scripts\\deploy.ps1",
        "Parameters": "",
        "TaskKillCommand": ""
      }
    ]
  },
  "Clipper": {
    "WindowWidth": 660,
    "WindowHeight": 420,
    "Items": [
      {
        "Label": "GitHub PAT Token",
        "FilePath": "C:\\Secrets\\github-pat.txt"
      }
    ]
  }
}
```

### Field Reference

**Launcher Items:**

| Field | Description |
|---|---|
| `Label` | The display name shown in the Application column |
| `FilePath` | Full path to the file to launch. Use `\\` for backslashes. Wrap paths with spaces in `\"quotes\"`. |
| `Parameters` | Optional command-line arguments passed to the program (leave empty string if not needed) |
| `TaskKillCommand` | The command to stop the process. Leave empty if kill is not needed. |

**Clipper Items:**

| Field | Description |
|---|---|
| `Label` | The display name shown in the Label column |
| `FilePath` | Full path to the plain text file whose contents will be copied |

> **Paths with spaces:** If your file path contains spaces, wrap the entire path in escaped quotes:
> ```json
> "FilePath": "\"C:\\Program Files\\My App\\app.exe\""
> ```

### Save Behaviour and Backups

| Trigger | What happens |
|---|---|
| **💾 Save button** | Current config is backed up as `launcher-config_YYYYMMDDTHHMMSS.json`, then updated config is written |
| **Closing the window** | Config is saved silently — no backup created |
| **Manual JSON edit** | Changes take effect on the next app restart |

Each window (Launcher and Clipper) independently reads the full config file on open and writes only its own section back — so editing Clipper settings never affects your Launcher list and vice versa.

### What Gets Remembered

| Setting | Remembered? |
|---|---|
| Application list (names, paths, kill commands) | ✅ Yes |
| Clipper list (names, file paths) | ✅ Yes |
| Window width and height | ✅ Yes (both windows separately) |
| Column widths | ✅ Yes (all 5 columns) |
| Status badges (Running/Idle/etc.) | ❌ No — always resets to Idle on restart |
| Whether Editing was ON or OFF | ❌ No — always starts locked (OFF) |

---

## 11. Help Button

The **? Help** link in the toolbar opens this `README.md` file directly in your default Markdown or text viewer (e.g. VS Code, Notepad++, or Notepad).

If the README cannot be found, a message appears in the status bar at the bottom of the window.

---

## 12. Tips and Tricks

- **Resize columns** — drag the column dividers in the header row to resize any column. Widths are saved automatically next time you use **💾 Save** or close the window.
- **Resize the window** — drag the window edges. The size is saved and restored on next launch.
- **Lock before closing** — after editing, click **🔒 Editing: OFF** and **💾 Save** before closing to protect your config.
- **Paths with spaces** — if an application path contains spaces (like `C:\Program Files\...`), wrap the whole path in escaped quotes in the JSON: `"\"C:\\Program Files\\...\\app.exe\""`.
- **No process name for a row?** — leave `TaskKillCommand` as `""` (empty). That row will always show `— Idle` and the Kill button will do nothing.
- **Keep secrets safe** — Clipper source files (tokens, passwords) are in your `Resources\` folder which is listed in `.gitignore`. They are never committed to source control.
- **Multiple backup files** — each press of 💾 Save creates a new timestamped backup. Periodically delete old `launcher-config_*.json` files to keep the folder clean.
- **Running `.ps1` scripts** — PowerShell execution policy is automatically bypassed for launched `.ps1` files, so they run without needing extra configuration.

---

## 13. Troubleshooting

### The app won't start — "You must install .NET to run this application"
Install the **.NET 8 Windows Desktop Runtime** from [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). Choose the "Windows Desktop Runtime" section, not the SDK.

### My config is gone / the table is empty
- Config is stored next to the executable. If you moved the `.exe` to a new folder, the config stays in the old folder. Copy `launcher-config.json` alongside the `.exe`.
- If running via `dotnet run`, the config lives in `bin\Debug\net8.0-windows\` — not the project root.

### A Launch button does nothing / shows ⚠ Error
- Check that the file path in the **Program/Script** column is correct and the file actually exists.
- Paths with spaces must be wrapped in escaped quotes in the JSON config.
- For `.ps1` files, ensure the full path is correct and the file is accessible.
- For `.py` files, ensure Python is installed and `python` is available on your system PATH.

### The Kill button does nothing
- The row must have a **TaskKillCommand** set in `launcher-config.json`. If the field is empty, the Kill button has no effect.
- Check that the process name in the command matches the actual `.exe` name (case-insensitive, but must be exact).

### Status always shows `— Idle` even though the app is running
- Auto-polling only works for rows with a TaskKill Command set (it needs a process name to look up).
- If you just launched the app, wait up to 5 seconds for the first poll cycle.

### The taskbar icon looks wrong / shows a blank icon
Run these commands in PowerShell, then unpin and re-pin the app:
```powershell
dotnet clean; dotnet build
taskkill /f /im explorer.exe
Remove-Item -Force "$env:LOCALAPPDATA\IconCache.db" -ErrorAction SilentlyContinue
Remove-Item -Force "$env:LOCALAPPDATA\Microsoft\Windows\Explorer\iconcache*.db" -ErrorAction SilentlyContinue
Start-Process explorer.exe
```

### I accidentally deleted a row / changed something wrong
Your last **💾 Save** created a timestamped backup (e.g. `launcher-config_20260508T143000.json`). Copy its contents back into `launcher-config.json` in any text editor to restore.

### The ? Help link says "README.md not found"
The Help button looks for `README.md` in the project root folder (3 levels above the `bin\Debug\net8.0-windows\` folder). If you moved the `.exe` to a completely different location without the README, it won't be found. This does not affect any other functionality.

---

## 14. Project Structure (For Developers)

```
AppsLauncher/
├── App.xaml                     # WPF application entry point (XAML)
├── App.xaml.cs                  # Application startup code
├── AppConfig.cs                 # Shared JSON config model — LauncherSection + ClipperSection
├── LaunchItem.cs                # Data model for one launcher row (Label, FilePath, Parameters, TaskKillCommand, Status)
├── ClipItem.cs                  # Data model for one Clipper row (Label, FilePath)
├── MainWindow.xaml              # Main launcher UI layout (WPF XAML)
├── MainWindow.xaml.cs           # Main window logic — launch, kill, polling, config load/save, edit lock
├── ClipperWindow.xaml           # Clipper window UI layout
├── ClipperWindow.xaml.cs        # Clipper window logic — copy-to-clipboard, config load/save
├── launcher.ico                 # Multi-resolution application icon (16/32/48/256 px)
├── AppsLauncher.csproj          # .NET 8 WPF project file
├── README.md                    # This file
├── .gitignore                   # Ignores bin/, obj/, Resources/, launcher-config*.json
└── Resources/                   # Runtime-only local files (gitignored — never committed)
    ├── *.bat, *.rdp, *.vbs ...  # Script and shortcut targets for the launcher
    └── Clipper-*                # Plain-text clipboard snippet files
```

**Technology Stack:**
- **.NET 8 WPF** (`net8.0-windows`, `UseWPF=true`)
- **DataGrid with DataGridTemplateColumn** — all columns use custom templates
- **ObservableCollection + INotifyPropertyChanged** — live data binding
- **System.Text.Json** — configuration serialization
- **PeriodicTimer + Task.Run** — background 5-second process status polling
- **Process.GetProcessesByName** — live process status detection

---

## 15. Build Commands Reference (For Developers)

```powershell
# Run directly from source (development mode)
dotnet run

# Debug build
dotnet build

# Release build
dotnet build -c Release

# Framework-dependent publish (smaller output; requires .NET 8 Desktop Runtime on target PC)
dotnet publish -c Release -r win-x64 --self-contained false

# Self-contained single-file executable (no runtime needed on target PC; larger file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Clean build outputs
dotnet clean
```

---

## .gitignore Highlights

```
bin/                      # Build outputs — never commit
obj/                      # Intermediate build files — never commit
Resources/                # Local scripts and secrets — never commit
launcher-config*.json     # Runtime config and timestamped backups — never commit
```
