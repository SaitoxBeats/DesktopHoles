# Desktop Holes

Desktop Holes is a lightweight Windows tray app that creates an edge "hole" (a reserved strip) on your desktop.
It uses Windows AppBar APIs to dock a borderless form to a monitor edge, so regular windows stay out of that area.

[![Demo Video](https://file.garden/Z5fpJocFXF3TjRhg/desktopholesVideosThumb.png)](https://file.garden/Z5fpJocFXF3TjRhg/DesktopHoles%20Video.mp4)

## What the app does

- Runs in the system tray.
- Lets you draw a selection rectangle over the desktop.
- Converts that selection into a hole anchored to the nearest monitor edge.
- Keeps the hole fixed while desktop/taskbar layout changes.
- Allows removing the hole instantly from tray menu, hotkey, or tray left-click toggle.

## How it works

1. `Program.cs` starts a WinForms tray-only app (`TrayAppContext`).
2. `TrayAppContext` creates:
   - A tray icon + context menu.
   - Global hotkeys:
     - `Ctrl + Win + Alt + L`: create/reset hole.
     - `Ctrl + Win + Alt + P`: remove hole.
   - A startup welcome dialog.
3. When creating a hole:
   - `SelectionForm` opens fullscreen across the virtual desktop.
   - You drag to select a rectangle.
   - `MaskForm.TrySetHole(...)` validates and snaps it to the nearest edge.
4. `MaskForm` registers as an AppBar (`SHAppBarMessage`) and reserves that strip of space on the selected monitor.
5. On layout changes (`ABN_POSCHANGED`), it reapplies position to stay correctly docked.

## Selection rules and limits

- The selected rectangle must be close to a monitor edge (within 24 px).
- Only one hole exists at a time.
- Hole thickness is taken from your selection and clamped to monitor size.
- Minimum thickness is 2 px.

## Build on your own PC (Windows)

### Prerequisites

- Windows 10 or newer.
- .NET 8 SDK installed (`dotnet --version` should return `8.x`).

### Build with .NET CLI

1. Clone the repository:

```powershell
git clone https://github.com/SaitoxBeats/DesktopHoles.git
cd DesktopHoles
```

2. Restore dependencies:

```powershell
dotnet restore
```

3. Build Release binaries:

```powershell
dotnet build DesktopHoles.sln -c Release
```

4. Run from source:

```powershell
dotnet run --project DesktopHoles.csproj -c Release
```

## Usage quick start

- Launch the app.
- Press `Ctrl + Win + Alt + L` (or use tray menu "Create New Hole...").
- Drag a strip near a monitor edge and release.
- To remove it, press `Ctrl + Win + Alt + P`, left-click tray icon, or choose "Remove Hole".
