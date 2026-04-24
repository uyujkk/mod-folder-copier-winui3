# CHANGELOG / 更新日志

This document records release changes in both Chinese and English.

本文档用于用中文和英文记录版本更新内容。

## Versioning Rule / 版本规则

- The version sequence starts from `2.2.2`.
- Future updates should increment the last number first: `2.2.3`, `2.2.4` ... `2.2.9`.
- When the last number reaches 10, carry to the previous one: `2.3.0`, `2.3.1` ...

- 版本从 `2.2.2` 开始。
- 后续更新优先递增最后一位：`2.2.3`、`2.2.4` ... `2.2.9`。
- 当最后一位满 `10` 时向前进一位：`2.3.0`、`2.3.1` ...

## v3.0

### 中文

- 引入多仓库工作区架构，支持仓库创建、编辑、重命名、删除和切换
- 新增仪表板、仓库页、在线页、更新页和设置页的整体导航结构
- 在线 Mod 页面已接入 GameBanana 分类读取、分页浏览、筛选、详情查看、下载与解压流程
- 在线安装后会记录来源链接、远程 ID、预览图和更新时间，用于后续 Mod 更新追踪
- 更新模块已启用，可手动检查已追踪 Mod 更新，并设置检查频率
- 完成一轮中文界面整理与乱码修复，主界面、弹窗和状态提示已基本恢复正常中文显示
- 显示版本更新为 `v3.0`，文件版本更新为 `3.0.0.0`

### English

- Introduced a multi-repository workspace architecture with create, edit, rename, delete, and switch actions
- Added a complete navigation shell with Dashboard, Repository, Online, Updates, and Settings views
- Connected the online mod page to GameBanana category browsing, paging, filtering, detail preview, download, and extraction
- Added source-link, remote-ID, preview-image, and update-time tracking after online installation
- Enabled the mod updates module with manual checks and configurable check frequency
- Completed a round of Chinese UI cleanup and mojibake fixes across the main interface, dialogs, and status text
- Updated the displayed app version to `v3.0` and the file version to `3.0.0.0`

## v2.2.8

### 中文

- 顶部压缩包导入按钮文案调整为“导入到当前选中文件夹”，并统一为导入到当前选中的第一层文件夹
- 保留第二层区域拖拽压缩包后自动解压导入的流程，导入逻辑更清晰
- 新增点击输入框外部区域时清除输入焦点的交互，便于退出光标与选中文本状态
- 优化快捷键输入框高亮逻辑，只有当前选中的快捷键框才会高亮显示
- 显示版本更新为 `v2.2.8`，文件版本更新为 `2.2.8.0`

### English

- Renamed the top archive import action to `Import To Selected Folder` and kept it targeted at the currently selected first-level folder
- Preserved drag-and-drop archive extraction in the second-level area for faster mod importing
- Added click-outside focus clearing so text inputs can exit caret and text selection state more naturally
- Refined shortcut field highlighting so only the currently selected shortcut box is highlighted
- Updated the displayed app version to `v2.2.8` and the file version to `2.2.8.0`

## v2.2.7

### 中文

- 调整信息显示模块宽度与下方主模块对齐，整体排版更整齐
- 修复深色模式下快捷键与描述输入框发白、显示不协调的问题
- 选中第一层文件夹后，支持将压缩包拖拽到第二层区域并自动解压导入
- 统一压缩包导入逻辑：若压缩包内只有单一顶层文件夹则直接导入，否则按压缩包文件名创建新的第二层 Mod 文件夹
- 显示版本更新为 `v2.2.7`，文件版本更新为 `2.2.7.0`

### English

- Aligned the information panel with the main modules below for a cleaner overall layout
- Fixed the shortcut and description input boxes appearing bright or inconsistent in dark mode
- Added drag-and-drop archive import to the second-level area after selecting a first-level folder
- Unified archive import behavior: a single top-level folder is imported directly, otherwise a new second-level mod folder is created from the archive file name
- Updated the displayed app version to `v2.2.7` and the file version to `2.2.7.0`

## v2.2.6

### 中文

- 第一层模块的“新建”和“重命名”操作改为右上角小图标按钮
- 第二层模块的“删除 Mod”操作改为右上角小图标按钮
- 调整模块标题与操作区排版，让面板更紧凑
- 图标按钮新增悬浮提示，鼠标停留时显示对应功能
- 图标按钮补充无障碍名称，提升键盘和辅助工具可读性
- 显示版本更新为 `v2.2.6`，文件版本更新为 `2.2.6.0`

### English

- Moved the first-level `New` and `Rename` actions to compact icon buttons in the top-right corner
- Moved the second-level `Delete Mod` action to a compact icon button in the top-right corner
- Refined the header and action layout for a cleaner, tighter module design
- Added hover tooltips so each icon button clearly shows its function
- Added accessibility names for the icon buttons to improve keyboard and assistive-tool support
- Updated the displayed app version to `v2.2.6` and the file version to `2.2.6.0`

## v2.2.5

### 中文

- 第一层文件夹新增“新建文件夹”功能，可直接在 Mod 存储目录下创建新的分类目录
- 第一层文件夹新增“重命名”功能，可直接修改当前分类目录名称
- 第二层区域新增“删除当前 Mod”功能
- 删除第二层 Mod 前会弹出确认窗口，避免误删
- 显示版本更新为 `v2.2.5`，文件版本更新为 `2.2.5.0`

### English

- Added a `New Folder` action for first-level folders to create new category folders directly inside the mod storage directory
- Added a `Rename` action for first-level folders to rename the current category folder
- Added a `Delete Mod` action to the second-level section
- Added a confirmation dialog before deleting a second-level mod to reduce accidental removal
- Updated the displayed app version to `v2.2.5` and the file version to `2.2.5.0`

## v2.2.4

### 中文

- 新增符号键支持，修复 `/` 等按键显示成数字键值的问题
- 快捷键说明文案同步更新，明确支持单键、组合键和符号键
- 压缩文件导入入口改为作用于当前第一层分类目录，不再导入到当前第二层 Mod
- 压缩文件导入扩展为支持更多格式，包括 `.zip`、`.7z`、`.tar`、`.gz`、`.tgz`、`.bz2`、`.xz`
- 新增第一层文件夹搜索框，可直接筛选分类目录
- 底部作者文案改为“工具作者”
- 显示版本更新为 `v2.2.4`，文件版本更新为 `2.2.4.0`

### English

- Added symbol key support for shortcut capture and fixed `/` and similar keys being shown as raw numeric key codes
- Updated the shortcut helper text to clarify support for single keys, key combinations, and symbol keys
- Changed archive import so it now targets the currently selected first-level category folder instead of the second-level mod folder
- Expanded archive import support to more formats, including `.zip`, `.7z`, `.tar`, `.gz`, `.tgz`, `.bz2`, and `.xz`
- Added a search box for first-level folders to filter category entries quickly
- Changed the footer author label to `Tool author`
- Updated the displayed app version to `v2.2.4` and the file version to `2.2.4.0`

## v2.2.3

### 中文

- 优化第二层列表显示，移除名称后面的数字列，只保留更清晰的名称和状态
- 第二层名称区域支持单行省略显示，避免长名称挤压布局
- 快捷键区域调整为 `1/3` 快捷键加 `2/3` 描述的比例，更适合录入和查看
- 快捷键说明文案更新，明确支持单键和组合键录入
- 顶部路径区命名更新为“Mod 存储文件夹”和“目标文件夹”，界面表意更清晰
- 显示版本更新为 `v2.2.3`，文件版本更新为 `2.2.3.0`

### English

- Refined the second-level list layout by removing the trailing number column and keeping a cleaner name-and-state view
- Added single-line ellipsis trimming for long second-level mod names to avoid layout crowding
- Adjusted the shortcut section to a `1/3` shortcut field and `2/3` description field layout for better readability
- Updated the shortcut help text to clarify that both single keys and key combinations are supported
- Renamed the top path labels to `Mod Storage` and `Target Folder` for clearer wording
- Updated the displayed app version to `v2.2.3` and the file version to `2.2.3.0`

## v2.2.2

### 中文

- 新增中英切换功能，界面标题、按钮、说明、状态和弹窗支持中英文切换
- 调整主界面布局，减少文字拥挤和底部说明被裁切的问题
- 在图片预览下方新增 Mod 链接模块，每个第二层 Mod 都可以绑定独立链接
- 新增快速访问按钮，可直接调用系统默认浏览器打开当前 Mod 对应链接
- 固定图片预览区域高度，避免不同图片切换时窗口内容跳动
- 配置文件新增保存语言设置和每个 Mod 的链接信息
- 显示版本更新为 `v2.2.2`，文件版本更新为 `2.2.2.0`

### English

- Added a Chinese / English language toggle for titles, buttons, hints, status text, and dialogs
- Adjusted the main layout to reduce crowding and prevent helper text from being clipped
- Added a mod link section below the preview area, with a dedicated link for each second-level mod
- Added a quick access button that opens the current mod link with the system default browser
- Fixed the preview area height so the layout no longer jumps when switching between images
- The config file now stores both the selected language and per-mod links
- Updated the app version to `v2.2.2` and the file version to `2.2.2.0`
