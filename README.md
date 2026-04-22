# [中文](./README.md) | [English](./README.en.md)

# Mod 文件复制器 WinUI 3 v2.2.8

> Non-official notice:
> This project is an unofficial fan-made tool. It is not affiliated with, endorsed by, or sponsored by XXMI, any game publisher, or any related developers.

这是一个运行在 Windows 上的 WinUI 3 图形工具，用于管理两层 Mod 文件夹、复制或移除第二层 Mod、导入压缩文件、记录每个 Mod 的快捷键与描述、设置预览图、绑定 Mod 链接，并快速启动外部启动器。

当前版本：

- 显示版本：`v2.2.8`
- 文件版本：`2.2.8.0`

## 主要功能

- 浏览 Mod 存储目录中的第一层分类文件夹和第二层 Mod 文件夹
- 选中第二层 Mod 后显示默认预览图
- 双击第二层 Mod 或点击操作按钮即可复制或移除
- 支持将压缩包导入到当前选中的第一层文件夹
- 支持在第二层区域拖拽压缩包并自动解压导入
- 支持 `.zip`、`.7z`、`.tar`、`.gz`、`.tgz`、`.bz2`、`.xz`
- 每个 Mod 可单独记录快捷键和描述
- 支持单键、组合键和符号键录入
- 支持把图片拖入预览区域设置当前 Mod 预览图
- 每个 Mod 可绑定独立网页链接，并用默认浏览器快速打开
- 支持配置并运行外部启动器
- 支持浅色 / 深色主题切换
- 支持中文 / English 界面切换

## v2.2.8 更新

- 顶部压缩包导入按钮文案调整为“导入到当前选中文件夹”
- 导入压缩包时会直接导入到当前选中的第一层文件夹
- 保留第二层区域拖拽压缩包自动解压导入
- 点击输入框外部区域时可清除输入焦点，退出光标和选中文本状态
- 快捷键输入框高亮逻辑调整为只在当前选中的快捷键框上显示

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
8. 根据需要执行复制、导入、预览图设置、快捷键记录或链接绑定

## 压缩包导入说明

- 顶部按钮会把压缩包导入到当前选中的第一层文件夹
- 第二层区域支持直接拖拽压缩包导入
- 如果压缩包内只有一个顶层文件夹，会直接按该文件夹导入
- 如果压缩包内是散文件或多个目录，会按压缩包文件名创建新的文件夹再导入

## 构建

当前仓库可通过以下命令构建：

```powershell
cmd /c build_winui.bat
```

构建完成后会生成：

- `dist/ModFolderCopier.exe`
- `dist/WinUI3`

## 更新日志

完整历史更新请见：

- [CHANGELOG.md](./CHANGELOG.md)
