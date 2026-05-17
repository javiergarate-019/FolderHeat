# FolderHeat – Project Summary

## Overview

**FolderHeat** is a Windows productivity tray utility that keeps the user's most relevant folders one shortcut away.

The core idea is **not just “recent folders”**, but **smart folder prioritization** based on a mix of:

* Recent activity
* Frequency of use
* Current work context
* Session behavior
* Manual pinning
* Time decay

The goal is to feel “intelligent”, surfacing the folders the user is most likely to want **right now**.

Example:

If the user is actively working on:

* `D:\ERP\SQL`
* `D:\ERP\VB6`
* `D:\ERP\Deploy`

FolderHeat should prioritize those folders and related ones over something that was merely opened once recently.

---

## Tech Stack

### Platform

* Windows desktop application

### Language

* C#

### Framework

* .NET 8 (LTS)

### UI

* WinForms

Reason:
This is a tray utility and WinForms is simpler and more robust than WPF for:

* NotifyIcon
* global hotkeys
* lightweight popups
* Windows integration

### Storage

* SQLite

Reason:

* embedded
* single file database
* fast
* robust
* ideal for desktop apps

Do NOT overengineer the schema.

---

## Architecture

Use **Clean Architecture (lightweight)**.

Structure:

```text
/src
    FolderHeat.Domain
    FolderHeat.Application
    FolderHeat.Infrastructure
    FolderHeat.App

/tests
    FolderHeat.Domain.Tests
    FolderHeat.Application.Tests
```

Dependencies:

```text
App → Application → Domain
Infrastructure → Application / Domain
App → Infrastructure
```

The Domain layer must not depend on:

* WinForms
* SQLite
* Windows APIs
* filesystem APIs

---

## Project Purpose

FolderHeat is NOT:

* a file manager
* a launcher
* a filesystem indexer
* a Windows Explorer replacement

It is a **smart folder launcher** optimized for productivity.

The main UX goal:

> Press one shortcut and immediately see the folders you probably want right now.

---

## Core Concepts

### Recent folders

Folders used recently.

Simple chronological ordering.

Question answered:

> “What did I use recently?”

---

### Active folders

Folders that are part of the user's **current working context**.

Question answered:

> “What am I currently working on?”

This is more valuable than recency.

Example:

Opening many ERP-related folders during a session should create an “ERP context”.

---

### Heat score

Every folder has a dynamic score called **Heat**.

Heat is based on:

* recency
* frequency
* session repetition
* context relevance
* transitions
* manual pinning
* time decay

Heat naturally decreases over time.

Folders should cool down when unused.

---

## Folder Categories

The popup should show categories such as:

```text
🔥 Active Now
🕘 Recent
⭐ Frequent
📌 Pinned
```

---

## Detection Strategy

Do NOT intercept the whole filesystem.

Avoid noisy approaches.

The app should infer **human work activity**, not system activity.

### Primary signals

#### 1. Explorer tracking

Detect folders currently opened in Windows Explorer.

Preferred approach:
Use Shell APIs / Explorer windows enumeration.

This is the most reliable signal.

---

#### 2. Active windows

Inspect:

* active process
* window title

Infer work context.

Examples:

```text
Visual Studio Code - ERP
Notepad++ - script.sql
Microsoft Word - Contract.docx
```

---

#### 3. Recent sources (later versions)

Support MRUs and recent files from applications such as:

* VS Code
* Notepad++
* Office

---

#### 4. Workspace context

If multiple related folders are being used:

```text
D:\ERP\SQL
D:\ERP\VB6
D:\ERP\Deploy
```

Then boost:

```text
D:\ERP
```

And potentially related folders.

This should feel predictive.

---

## Session Logic

Avoid noise.

Example:

If VS Code opens 100 files in 2 seconds:

This counts as **one human interaction**, not 100.

Introduce session grouping.

Repeated accesses within a short interval should not endlessly increase score.

---

## Heat Engine Ideas

The ranking should combine:

* recent activity
* long-term usage
* short-term momentum
* current context

Possible inspiration:
Hacker News / Reddit ranking logic.

Simple example:

```text
score =
    recentWeight +
    frequencyWeight +
    sessionWeight +
    contextWeight +
    pinnedBoost -
    decay
```

Keep it simple initially.

Avoid premature optimization.

---

## Manual Controls

The app should support manual override.

Users must be able to:

### Add folder

Manually add a folder.

### Pin folder

Always prioritize.

### Ignore folder

Exclude noisy or unwanted folders.

---

## Tray UX

### Left click / Hotkey

Open main popup.

### Right click

Show tray menu:

```text
Open FolderHeat
Add current folder
Add folder...
Pinned folders
Ignored folders
Settings
Exit
```

### Add current folder

Detect current context:

Priority:

1. Explorer current folder
2. VS Code workspace
3. inferred active folder
4. fallback folder picker

Avoid modifying Windows Explorer context menus initially.

Keep integration minimal.

---

## Hotkey

Global shortcut:

Suggested default:

```text
Ctrl + Alt + Space
```

Must be configurable later.

---

## Persistence

Store data locally.

Examples of data:

* folder path
* last access
* heat
* pinned
* ignored
* transition weights
* access count

Keep schema simple.

---

## UI Philosophy

Fast.

Minimal.

Keyboard-first.

The app should feel instant.

Open popup → type → press Enter → folder opens.

No heavy UI.

No ribbon-style UX.

No clutter.

---

## Version Roadmap

### v0.1

* tray icon
* global hotkey
* popup
* manual add folder
* pinned folders
* recent folders
* SQLite storage

### v0.2

* Explorer detection
* heat score
* active folders
* exclusions

### v0.3

* VS Code support
* Notepad++
* Office recent sources

### v0.4

* context prediction
* transitions
* likely next folders

---

## Coding Preferences

* Clean Architecture
* SOLID without overengineering
* avoid unnecessary abstractions
* clear naming
* readable code
* comments only when useful
* English naming everywhere

Prefer pragmatic solutions over academic complexity.
