# FolderHeat Architecture

FolderHeat uses lightweight Clean Architecture.

```text
FolderHeat.App -> FolderHeat.Application -> FolderHeat.Domain
FolderHeat.App -> FolderHeat.Infrastructure
FolderHeat.Infrastructure -> FolderHeat.Application / FolderHeat.Domain
```

## Domain

Owns core entities and scoring rules.

Current responsibilities:

- `FolderEntry`
- `FolderHeatScore`

Domain must stay free of WinForms, SQLite, shell APIs, and other infrastructure concerns.

## Application

Owns use cases and contracts.

Current responsibilities:

- add folders
- open folders
- pin/unpin folders
- ignore folders
- produce popup groups
- define repository, clock, and launcher abstractions

## Infrastructure

Owns external implementation details.

Current responsibilities:

- SQLite persistence
- system clock
- Windows folder launching

v0.3 Explorer detection should live here behind an application contract.

Current context sources:

- Explorer windows
- VS Code recent workspaces/files
- Notepad++ session files
- Windows recent shortcuts

## App

Owns WinForms and tray integration.

Current responsibilities:

- `NotifyIcon`
- global hotkey
- popup form
- folder picker
- user commands

The app project should orchestrate use cases, not contain ranking, persistence, or Explorer-detection logic.

Current app-owned UI:

- tray menu
- popup
- ignored folders dialog
- settings dialog
- hotkey registration
