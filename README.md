# [中文](./README.md) | [English](./README.en.md)

# 集成化mod管理器 v3.0

> Non-official notice:
> This project is an unofficial fan-made tool. It is not affiliated with, endorsed by, or sponsored by XXMI, any game publisher, or any related developers.

这是一个运行在 Windows 上的 WinUI 3 图形工具，用于管理多仓库 Mod 工作区、浏览和操作两层 Mod 文件夹、导入压缩文件、记录每个 Mod 的快捷键与描述、设置预览图、绑定 Mod 链接，并接入在线 Mod 浏览、下载与更新追踪能力。

当前版本：

- 显示版本：`v3.0`
- 文件版本：`3.0.0.0`

## 系统要求

- 操作系统：Windows 10 64 位（建议 2004 / 20H1 及以上）或 Windows 11 64 位
- 处理器架构：`x64`
- 不支持：32 位 Windows（`x86`）
- 建议分辨率：`1920×1080` 或更高
- 可选组件：如需从程序内启动外部启动器，请准备好对应的启动器可执行文件

## 部署环境要求

如果你只需要运行发布包：

- 直接使用 `dist/ModFolderCopier.exe`
- 同目录下需要保留 `dist/WinUI3/` 运行文件夹

如果你需要在本地编译源码：

- 操作系统：Windows 10 / 11
- .NET SDK：`8.x`
- Visual Studio 2022 或 Build Tools 2022
- Windows SDK：建议 `10.0.22621` 或更新版本
- Git：用于拉取和同步仓库
- 构建命令：`cmd /c build_winui.bat`

## 主要功能

- 支持多仓库工作区，可分别配置 Mod 仓库路径、目标路径和启动器路径
- 支持仪表板查看仓库总览、路径状态和基础统计
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
- 支持在线 Mod 浏览、分页、筛选、详情查看、下载与解压
- 支持记录在线来源链接、远程 ID、预览图和更新时间
- 支持 Mod 更新检查，并可设置检查频率或手动检查
- 支持浅色 / 深色主题切换
- 支持中文 / English 界面切换

## v3.0 更新

- 引入多仓库工作区架构，支持仓库创建、编辑、重命名、删除和切换
- 新增仪表板、仓库页、在线页、更新页和设置页的整体导航结构
- 在线 Mod 页面已接入 GameBanana 分类读取、分页浏览、筛选、详情查看、下载与解压流程
- 在线安装后会记录来源链接、远程 ID、预览图和更新时间，用于后续 Mod 更新追踪
- 更新模块已启用，可手动检查已追踪 Mod 更新，并设置检查频率
- 完成一轮中文界面整理与乱码修复，主界面、弹窗和状态提示已基本恢复正常中文显示

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

- `beta测试/dist/ModFolderCopier.exe`

WinUI 主程序：

- `beta测试/dist/WinUI3/ModFolderCopier.WinUI.exe`

通常直接运行 `ModFolderCopier.exe` 即可。

## 基本使用

1. 运行 `ModFolderCopier.exe`
2. 设置 `Mod 存储文件夹`
3. 设置 `目标文件夹`
4. 如有需要，设置 `启动器`
5. 点击 `刷新目录`
6. 选择第一层分类
7. 选择第二层 Mod
8. 根据需要执行复制、导入、预览图设置、快捷键记录、链接绑定或在线 Mod 安装

## 压缩包导入说明

- 顶部按钮会把压缩包导入到当前选中的第一层文件夹
- 第二层区域支持直接拖拽压缩包导入
- 如果压缩包内只有一个顶层文件夹，会直接按该文件夹导入
- 如果压缩包内是散文件或多个目录，会按压缩包文件名创建新的文件夹再导入

## 构建

当前 Beta 工作区可通过以下命令构建：

```powershell
cd beta测试
cmd /c build_winui.bat
```

构建完成后会生成：

- `beta测试/dist/ModFolderCopier.exe`
- `beta测试/dist/WinUI3`

## 更新日志

完整历史更新请见：

- [CHANGELOG.md](./CHANGELOG.md)
