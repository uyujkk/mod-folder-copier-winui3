[中文](./README.md) | [English](./README.en.md)

# Integrated Mod Manager v3.0

> Non-official notice:
> This project is an unofficial fan-made tool. It is not affiliated with, endorsed by, or sponsored by XXMI, any game publisher, or any related developers.

This is a WinUI 3 desktop tool for Windows that helps manage multi-repository mod workspaces, operate on two-level mod folders, import archive files, record per-mod shortcut notes and descriptions, set preview images, bind mod links, and add online mod browsing, download, and update tracking workflows.

Current version:

- Display version: `v3.0`
- File version: `3.0.0.0`

## Main Features

- Support multiple repository workspaces, each with its own mod source path, target path, and launcher path
- Provide a dashboard for repository overview, path status, and basic statistics
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
- Browse online mods, paginate, filter, open details, download, and extract
- Record source links, remote IDs, preview images, and update timestamps for online-installed mods
- Check tracked mod updates manually or on a schedule
- Support light / dark theme switching
- Support Chinese / English UI switching

## What's New In v3.0

- Introduced a multi-repository workspace architecture with create, edit, rename, delete, and switch actions
- Added a complete navigation shell with Dashboard, Repository, Online, Updates, and Settings views
- Connected the online mod page to GameBanana category browsing, paging, filtering, detail preview, download, and extraction
- Added source-link, remote-ID, preview-image, and update-time tracking after online installation
- Enabled the mod updates module with manual checks and configurable check frequency
- Completed a round of Chinese UI cleanup and mojibake fixes across the main interface, dialogs, and status text

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

- `beta测试/dist/ModFolderCopier.exe`

WinUI app:

- `beta测试/dist/WinUI3/ModFolderCopier.WinUI.exe`

In most cases, just run `ModFolderCopier.exe`.

## Basic Usage

1. Run `ModFolderCopier.exe`
2. Set the `Mod Storage Folder`
3. Set the `Target Folder`
4. Optionally set the `Launcher`
5. Click `Refresh`
6. Choose a first-level category
7. Choose a second-level mod
8. Double-click the mod, use the copy button, or install an online mod into the current repository

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

- `beta测试/WinUI3/`
- `beta测试/WinUILauncher.cs`
- `beta测试/build_winui.bat`

Build command:

```powershell
cd beta测试
cmd /c build_winui.bat
```

## Changelog

For full update history, see:

- [CHANGELOG.md](./CHANGELOG.md)
