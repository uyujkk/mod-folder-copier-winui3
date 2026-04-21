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

## v2.2.3

### 中文

- 优化第二层列表显示，移除名称后面的数字列，只保留更清晰的名称和状态。
- 第二层名称区域支持单行省略显示，避免长名称挤压布局。
- 快捷键区域调整为 `1/3` 快捷键加 `2/3` 描述的比例，更适合录入和查看。
- 快捷键说明文案更新，明确支持单键和组合键录入。
- 顶部路径区命名更新为“Mod 存储文件夹”和“目标文件夹”，界面表意更清晰。
- 同步更新显示版本为 `v2.2.3`，文件版本为 `2.2.3.0`。

### English

- Refined the second-level list layout by removing the trailing number column and keeping a cleaner name-and-state view.
- Added single-line ellipsis trimming for long second-level mod names to avoid layout crowding.
- Adjusted the shortcut section to a `1/3` shortcut field and `2/3` description field layout for better readability.
- Updated the shortcut help text to clarify that both single keys and key combinations are supported.
- Renamed the top path labels to `Mod Storage` and `Target Folder` for clearer wording.
- Updated the displayed app version to `v2.2.3` and the file version to `2.2.3.0`.

## v2.2.2

### 中文

- 新增中英切换功能，界面标题、按钮、说明、状态和弹窗支持中英文切换。
- 调整主界面布局，减少文字拥挤和底部说明被裁切的问题。
- 在图片预览下方新增 Mod 链接模块，每个第二层 Mod 都可以绑定独立链接。
- 新增快速访问按钮，可直接调用系统默认浏览器打开当前 Mod 对应链接。
- 固定图片预览区域高度，避免不同图片切换时窗口内容跳动。
- 配置文件新增保存语言设置和每个 Mod 的链接信息。
- 当前版本号更新为 `v2.2.2`，文件版本更新为 `2.2.2.0`。

### English

- Added a Chinese / English language toggle for titles, buttons, hints, status text, and dialogs.
- Adjusted the main layout to reduce crowding and prevent helper text from being clipped.
- Added a mod link section below the preview area, with a dedicated link for each second-level mod.
- Added a quick access button that opens the current mod link with the system default browser.
- Fixed the preview area height so the layout no longer jumps when switching between images.
- The config file now stores both the selected language and per-mod links.
- Updated the app version to `v2.2.2` and the file version to `2.2.2.0`.
