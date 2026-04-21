# [中文](./README.md) | [English](./README.en.md)

# Mod 文件复制器 WinUI 3 v2.2.6

> Non-official notice:
> This project is an unofficial fan-made tool. It is not affiliated with, endorsed by, or sponsored by XXMI, any game publisher, or any related developers.

这是一个运行在 Windows 上的 WinUI 3 图形化工具，用于管理两层 Mod 文件夹、复制或移除第二层 Mod、导入压缩文件、记录每个 Mod 的快捷键与描述、设置预览图、绑定 Mod 链接，并快速启动外部启动器。

当前版本：

- 显示版本：`v2.2.6`
- 文件版本：`2.2.6.0`

## 主要功能

- 浏览 Mod 存储文件夹中的第一层和第二层文件夹
- 选中第二层 Mod 后显示默认预览图
- 双击第二层 Mod 或点击操作按钮即可复制或移除
- 支持把压缩文件导入到当前第一层分类目录
- 支持 `.zip`、`.7z`、`.tar`、`.gz`、`.tgz`、`.bz2`、`.xz`
- 每个 Mod 可单独记录快捷键和描述
- 支持单键、组合键和符号键录入
- 支持拖动图片到预览区设置当前 Mod 预览图
- 每个 Mod 可绑定独立网页链接，并用默认浏览器快速访问
- 支持配置并运行外部启动器
- 支持浅色 / 深色主题切换
- 支持中文 / English 界面切换

## v2.2.6 更新

- 第一层模块的“新建”和“重命名”操作改为右上角小图标按钮
- 第二层模块的“删除 Mod”操作改为右上角小图标按钮
- 调整模块标题与操作区排版，让面板更紧凑
- 图标按钮新增悬浮提示
- 图标按钮补充无障碍名称

## 推荐目录结构

```text
Mod 存储文件夹
├─ 分类A
│  ├─ Mod1
│  ├─ Mod2
│  └─ Mod3
├─ 分类B
│  ├─ Mod4
│  └─ Mod5
└─ 分类C
   └─ Mod6
```

- 第一层：分类文件夹
- 第二层：实际操作的 Mod 文件夹

## 如何运行

主入口：

- `dist/ModFolderCopier.exe`

WinUI 主程序：

- `dist/WinUI3/ModFolderCopier.WinUI.exe`

通常直接运行 `ModFolderCopier.exe` 即可。

## 基本使用

1. 运行 `ModFolderCopier.exe`
2. 设置 `Mod 存储文件夹`
3. 设置 `目标文件夹`
4. 如有需要，设置 `启动器`
5. 点击 `刷新目录`
6. 选择第一层分类
7. 选择第二层 Mod
8. 双击 Mod 或点击复制按钮

行为说明：

- 如果目标文件夹里不存在同名目录，程序会复制该 Mod
- 如果目标文件夹里已存在同名目录，程序会移除该 Mod

## 界面说明

### 顶部路径区

- Mod 存储文件夹
- 目标文件夹
- 启动器

### 第一层模块

- 支持搜索第一层分类
- 右上角图标按钮支持新建和重命名

### 第二层模块

- 用于选择具体 Mod
- 右上角图标按钮支持删除当前 Mod
- 删除前会弹出确认窗口

### 快捷键与描述

- 每个 Mod 可以保存独立的快捷键和描述
- 支持单键，例如 `1`、`Q`、`F1`
- 支持组合键，例如 `Ctrl+1`
- 支持符号键，例如 `/`、`;`、`[`、`]`、`\`

### 预览图与链接

- 优先读取 `preview.*`、`cover.*`、`thumbnail.*`、`image.*`
- 如果没有这些命名，会尝试显示当前 Mod 文件夹中的第一张图片
- 可以把图片直接拖到预览区设置为新预览图
- 可以为当前 Mod 绑定链接，并点击按钮快速访问

## 配置文件

程序会在运行目录中保存配置，例如：

- `dist/WinUI3/config.ini`

配置内容包括：

- Mod 存储文件夹路径
- 目标文件夹路径
- 启动器路径
- 当前主题
- 当前语言
- 每个 Mod 的快捷键与描述
- 每个 Mod 的链接

## 构建

源码入口：

- `WinUI3/`
- `WinUILauncher.cs`
- `build_winui.bat`

构建命令：

```powershell
cmd /c build_winui.bat
```

## 更新日志

完整更新内容见：

- [CHANGELOG.md](./CHANGELOG.md)
