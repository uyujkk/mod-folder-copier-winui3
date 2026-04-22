[中文](./README.md) | [English](./README.en.md)

# Mod Folder Copier WinUI 3 v2.2.8

> Non-official notice:
> This project is an unofficial fan-made tool. It is not affiliated with, endorsed by, or sponsored by XXMI, any game publisher, or any related developers.

This is a WinUI 3 desktop tool for Windows that helps manage two-level mod folders, copy or remove second-level mods, import archive files, record per-mod shortcut notes and descriptions, set preview images, bind mod links, and quickly launch an external launcher.

Current version:

- Display version: `v2.2.8`
- File version: `2.2.8.0`

## Main Features

- Browse first-level and second-level folders inside the mod storage folder
- Show a preview image for the selected second-level mod
- Double-click a second-level mod or use the action button to copy or remove it
- Import archives into the currently selected first-level category folder
- Support `.zip`, `.7z`, `.tar`, `.gz`, `.tgz`, `.bz2`, and `.xz`
- Save shortcut notes and descriptions for each mod
- Support single keys, key combinations, and symbol keys for shortcut capture
- Drag and drop an image into the preview area to set the current mod preview
- Bind a dedicated link for each mod and open it in the default browser
- Configure and launch an external launcher
- Support light / dark theme switching
- Support Chinese / English UI switching

## What's New In v2.2.8

- Renamed the top archive import action to `Import To Selected Folder`
- Kept archive import targeted at the currently selected first-level folder
- Preserved drag-and-drop archive extraction in the second-level area for faster mod importing
- Added click-outside focus clearing so text inputs can exit caret and selection state more naturally
- Refined shortcut field highlighting so only the currently selected shortcut box is highlighted

## Recommended Folder Structure

```text
Mod Storage Folder
├─ CategoryA
│  ├─ Mod1
│  ├─ Mod2
│  └─ Mod3
├─ CategoryB
│  ├─ Mod4
│  └─ Mod5
└─ CategoryC
   └─ Mod6
```

- First level: category folders
- Second level: actual mod folders

## How To Run

Main launcher:

- `dist/ModFolderCopier.exe`

WinUI app:

- `dist/WinUI3/ModFolderCopier.WinUI.exe`

In most cases, just run `ModFolderCopier.exe`.

## Basic Usage

1. Run `ModFolderCopier.exe`
2. Set the `Mod Storage Folder`
3. Set the `Target Folder`
4. Optionally set the `Launcher`
5. Click `Refresh`
6. Choose a first-level category
7. Choose a second-level mod
8. Double-click the mod or click the copy button

Behavior:

- If the target folder does not contain a folder with the same name, the mod is copied
- If the target folder already contains a folder with the same name, the mod is removed

## Interface Overview

### Top Path Section

- Mod Storage Folder
- Target Folder
- Launcher

### First-Level Module

- Search first-level categories
- Use the top-right icon buttons to create or rename a category

### Second-Level Module

- Select a specific mod
- Use the top-right icon button to delete the selected mod
- A confirmation dialog appears before deletion

### Shortcut And Description

- Each mod can store its own shortcut and description
- Supports single keys such as `1`, `Q`, `F1`
- Supports key combinations such as `Ctrl+1`
- Supports symbol keys such as `/`, `;`, `[`, `]`, and `\`

### Preview And Link

- Checks `preview.*`, `cover.*`, `thumbnail.*`, and `image.*` first
- Falls back to the first image found in the current mod folder
- Lets you drag and drop an image into the preview area
- Lets you bind a web link for each mod and open it quickly

## Configuration

The app stores configuration data in:

- `dist/WinUI3/config.ini`

This includes:

- Mod storage folder path
- Target folder path
- Launcher path
- Theme
- Language
- Shortcut notes and descriptions for each mod
- Links for each mod

## Build

Source entry points:

- `WinUI3/`
- `WinUILauncher.cs`
- `build_winui.bat`

Build command:

```powershell
cmd /c build_winui.bat
```

## Changelog

For full update history, see:

- [CHANGELOG.md](./CHANGELOG.md)
