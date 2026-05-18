# FolderHeat

FolderHeat is a lightweight Windows tray utility that keeps your most relevant folders one shortcut away.

It is not a file manager and it is not an Explorer replacement. It is a smart folder launcher: press the hotkey, get a compact popup, and open the folder you probably need right now.

## Features

- Tray utility for Windows.
- Global hotkey, configurable from Settings.
- Popup grouped by `Active Now`, `Pinned`, `Recent`, and `Frequent`.
- Search by folder name or path.
- Manual folder add.
- Add current folder when FolderHeat can infer the active context.
- Pin, unpin, ignore, and restore folders.
- Smart context signals from Explorer, VS Code, Notepad, Notepad++, and Windows recent shortcuts.
- Related-folder and likely-next-folder ranking.
- Local SQLite storage under `%LOCALAPPDATA%\FolderHeat`.
- Optional start with Windows.
- About dialog with version, GitHub link, license, and diagnostics.

## Install

Download the latest portable ZIP from GitHub Releases:

```text
https://github.com/javiergarate/FolderHeat/releases
```

Extract the ZIP and run:

```text
FolderHeat.App.exe
```

FolderHeat runs in the system tray. The default shortcut is:

```text
Ctrl + Alt + Space
```

## Usage

- Left-click the tray icon or press the hotkey to open the popup.
- Type to filter folders.
- Press `Enter` or click `Open` to open the selected folder.
- Use `Pin` for folders that should stay easy to reach.
- Use `Ignore` for noisy folders that should not appear.
- Open Settings from the tray menu to change the shortcut or enable start with Windows.

Popup reasons:

- `Current`: detected from your active context.
- `Next`: usually opened after your current context.
- `Related`: related to the current folder context.
- `Pinned`: pinned by you.
- `Recent`: opened recently.
- `Frequent`: opened often.
- `Tracked`: known to FolderHeat, with no stronger signal yet.

## Privacy

FolderHeat stores its data locally.

By default, the database is stored at:

```text
%LOCALAPPDATA%\FolderHeat\folderheat.db
```

No cloud service is used by the app.

## Development

FolderHeat uses:

- C#
- .NET 10
- WinForms
- SQLite
- Lightweight Clean Architecture

Project structure:

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

Build:

```powershell
dotnet build FolderHeat.slnx
```

Test:

```powershell
dotnet test FolderHeat.slnx
```

Run from source:

```powershell
dotnet run --project src\FolderHeat.App
```

## Release

Release builds generate a local portable ZIP under `artifacts/`.
ZIP files are ignored by Git and should be published through GitHub Releases.

To publish a version:

```powershell
dotnet build src\FolderHeat.App\FolderHeat.App.csproj -c Release
git tag v0.7.0
git push origin main
git push origin v0.7.0
```

The GitHub Actions release workflow also builds the portable ZIP and attaches it to the release for pushed `v*` tags.

## License

MIT License. See [LICENSE](LICENSE).
