# FolderHeat Roadmap

## v0.1 - Scaffold

- Solution and project structure.
- Lightweight Clean Architecture boundaries.
- Domain and application test projects.

## v0.2 - Local Folder Launcher Foundation

Current implementation target:

- .NET 10 WinForms tray app.
- `Ctrl+Alt+Space` global hotkey.
- Popup list grouped by Active Now, Pinned, Recent, and Frequent.
- Manual folder add.
- SQLite persistence in the user's local app data folder.
- Open tracking through `FolderCatalogService`.
- Pin/unpin support.
- Ignore support in the application layer.
- Heat score based on pinning, access frequency, and recency.

This version is still intentionally simple. It does not try to inspect the whole filesystem and it does not replace Explorer.

## v0.3 - Work Context Signals

Current implementation target:

- Detect folders open in Windows Explorer.
- Add detected Explorer folders to the catalog without treating background noise as user intent.
- Boost currently open Explorer folders in Active Now.
- Add a tray command for adding the current folder when it can be inferred.
- Keep source detection behind application/infrastructure contracts so WinForms does not own context logic.
- Detect recent context from VS Code, Notepad++, and Windows recent shortcuts.
- Avoid duplicate folders across popup sections.
- Manage ignored folders.
- Configure the global hotkey.
- Show why a folder is ranked.
- Boost related folders and likely next folders from transition history.

## Later

- Stronger settings UI for scoring weights.
- More explicit source labels per context provider.
- Infrastructure-level tests around shell source parsing.
- Additional application MRU sources.
