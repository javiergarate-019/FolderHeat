# FolderHeat

FolderHeat is a lightweight Windows tray utility for launching the folders the user is most likely to need right now.

This implementation includes the v0.3 smart-context foundation:

- .NET 10 WinForms tray app.
- Lightweight Clean Architecture projects.
- SQLite persistence under `%LOCALAPPDATA%\FolderHeat\folderheat.db`.
- Global hotkey: `Ctrl+Alt+Space`.
- Popup with Active Now, Pinned, Recent, and Frequent groups.
- Manual folder add.
- Open folder tracking.
- Pin/unpin support.
- Ignore/restore support.
- Active context from Explorer, VS Code, Notepad++, and Windows recent shortcuts.
- Duplicate-free popup groups with rank reasons.
- Related-folder and likely-next-folder boosts.
- Configurable global hotkey.

## Projects

```text
src/
  FolderHeat.Domain
  FolderHeat.Application
  FolderHeat.Infrastructure
  FolderHeat.App

tests/
  FolderHeat.Domain.Tests
  FolderHeat.Application.Tests
```

## Build

```powershell
dotnet build FolderHeat.slnx
```

## Test

```powershell
dotnet test FolderHeat.slnx
```

## Run

```powershell
dotnet run --project src\FolderHeat.App
```
