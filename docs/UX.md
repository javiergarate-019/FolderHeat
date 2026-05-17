# FolderHeat UX

FolderHeat is a keyboard-first tray utility.

Primary workflow:

```text
press Ctrl+Alt+Space -> type a few letters -> press Enter
```

The popup should feel instant and should bias toward the folders the user probably wants right now.

## Current Popup

The v0.2 popup shows:

- Active Now
- Pinned
- Recent
- Frequent

Supported actions:

- add folder
- pin/unpin selected folder
- ignore selected folder
- open selected folder
- filter by name or path
- see the ranking reason

## Tray

Left click opens the popup.

Right click currently exposes:

- Open FolderHeat
- Add current folder
- Add folder...
- Ignored folders...
- Settings...
- Exit

## v0.3 UX Direction

Explorer and recent-source detection should improve Active Now without adding visible complexity.

Expected user-facing behavior:

- folders open in Explorer appear quickly in Active Now
- Add current folder uses Explorer context when available
- manual Add folder remains the fallback
- ignored folders can be restored
- the hotkey can be changed from Settings
- repeated folders do not appear in multiple sections at once

The app should stay lightweight. Avoid dashboards, file-manager UI, and long settings flows until the core ranking behavior is useful.
