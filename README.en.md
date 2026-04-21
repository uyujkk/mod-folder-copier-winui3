[中文](./README.md) | [English](./README.en.md)

# Mod Folder Copier WinUI 3 v2.2.3

> Non-official notice:  
> This project is an unofficial fan-made tool. It is not affiliated with, endorsed by, or sponsored by XXMI, any game publisher, or any related developers.

This is a WinUI 3 desktop tool for Windows that helps manage two-level mod folders, copy or remove second-level mods, import ZIP files, record per-mod shortcut notes and descriptions, set preview images, and quickly launch an external launcher.

Current version:

- Display version: `v2.2.3`
- File version: `2.2.3.0`

## 1. Main Features

- Browse first-level and second-level folders inside the mod storage folder
- Show a preview image for the selected second-level mod
- Double-click a second-level mod or use the action button to copy it into the target folder
- Remove the copied mod if a folder with the same name already exists in the target folder
- Import `.zip` files directly into the currently selected second-level mod folder
- Save shortcut notes and descriptions for each second-level mod
- Drag and drop an image into the preview area to set the current mod preview
- Bind a dedicated link for each mod and open it in the default browser
- Configure and launch an external launcher such as `XXMI Launcher`
- Support light / dark theme switching
- Support Chinese / English UI switching

## 2. Recommended Folder Structure

The app works with a two-level folder layout:

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

The second-level mod folder is the real target for copy, remove, ZIP import, preview, shortcut notes, and links.

## 3. How To Run

Main launcher:

- `dist/ModFolderCopier.exe`

WinUI app:

- `dist/WinUI3/ModFolderCopier.WinUI.exe`

In most cases, just run `ModFolderCopier.exe`.

## 4. Interface Overview

### Top Path Section

- Mod Storage Folder
- Target Folder
- Launcher Path

### Main Actions

- Refresh folders
- Import ZIP into current second-level mod
- Run launcher
- Copy current second-level folder

### Status Cards

- First-level folder count
- Second-level folder count
- Current copy status
- Current second-level folder name

### First-Level List

Used to select a category.

### Second-Level List

Used to select a specific mod. Double-clicking an entry will copy or remove that mod.

### Shortcut And Description Section

Each mod can store its own:

- Shortcut
- Description

Supported input:

- Single keys such as `1`, `Q`, `F1`
- Key combinations such as `Ctrl+1`

### Preview And Link Section

The preview area checks these image names first:

- `preview.*`
- `cover.*`
- `thumbnail.*`
- `image.*`

If none exist, it tries to display the first image found inside the current mod folder.

Each mod can also store its own link, and the quick access button opens it in the default browser.

## 5. Basic Usage

1. Run `ModFolderCopier.exe`
2. Set the mod storage folder
3. Set the target folder
4. Optionally set the launcher path
5. Click `Refresh`
6. Choose a first-level category
7. Choose a second-level mod
8. Double-click the mod or click the copy button

Behavior:

- If the target folder does not contain a folder with the same name, the mod is copied
- If the target folder already contains a folder with the same name, the mod is removed

## 6. ZIP Import

1. Select a second-level mod
2. Click the ZIP import button
3. Choose a `.zip` file

The archive contents will be extracted into the current mod folder.

## 7. Preview Images

You can either:

- Manually place preview files such as `preview.png`, `cover.jpg`, `thumbnail.webp`, or `image.png`
- Drag an image directly into the preview area

## 8. Configuration

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

## 9. Build

Source entry points:

- `WinUI3/`
- `WinUILauncher.cs`
- `build_winui.bat`

Build command:

```powershell
cmd /c build_winui.bat
```
