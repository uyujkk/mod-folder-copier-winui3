# CHANGELOG / 鏇存柊鏃ュ織

This document records release changes in both Chinese and English.

鏈枃妗ｇ敤浜庣敤涓枃鍜岃嫳鏂囪褰曠増鏈洿鏂板唴瀹广€?

## Versioning Rule / 鐗堟湰瑙勫垯

- The version sequence starts from `2.2.2`.
- Future updates should increment the last number first: `2.2.3`, `2.2.4` ... `2.2.9`.
- When the last number reaches 10, carry to the previous one: `2.3.0`, `2.3.1` ...

- 鐗堟湰浠?`2.2.2` 寮€濮嬨€?
- 鍚庣画鏇存柊浼樺厛閫掑鏈€鍚庝竴浣嶏細`2.2.3`銆乣2.2.4` ... `2.2.9`銆?
- 褰撴渶鍚庝竴浣嶆弧 `10` 鏃跺悜鍓嶈繘涓€浣嶏細`2.3.0`銆乣2.3.1` ...

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

### 涓枃

- 绗竴灞傛ā鍧楃殑鈥滄柊寤衡€濆拰鈥滈噸鍛藉悕鈥濇搷浣滄敼涓哄彸涓婅灏忓浘鏍囨寜閽?
- 绗簩灞傛ā鍧楃殑鈥滃垹闄?Mod鈥濇搷浣滄敼涓哄彸涓婅灏忓浘鏍囨寜閽?
- 璋冩暣妯″潡鏍囬涓庢搷浣滃尯鎺掔増锛岃闈㈡澘鏇寸揣鍑?
- 鍥炬爣鎸夐挳鏂板鎮诞鎻愮ず锛岄紶鏍囧仠鐣欐椂鏄剧ず瀵瑰簲鍔熻兘
- 鍥炬爣鎸夐挳琛ュ厖鏃犻殰纰嶅悕绉帮紝鎻愬崌閿洏鍜岃緟鍔╁伐鍏峰彲璇绘€?
- 鏄剧ず鐗堟湰鏇存柊涓?`v2.2.6`锛屾枃浠剁増鏈洿鏂颁负 `2.2.6.0`

### English

- Moved the first-level `New` and `Rename` actions to compact icon buttons in the top-right corner
- Moved the second-level `Delete Mod` action to a compact icon button in the top-right corner
- Refined the header and action layout for a cleaner, tighter module design
- Added hover tooltips so each icon button clearly shows its function
- Added accessibility names for the icon buttons to improve keyboard and assistive-tool support
- Updated the displayed app version to `v2.2.6` and the file version to `2.2.6.0`

## v2.2.5

### 涓枃

- 绗竴灞傛枃浠跺す鏂板鈥滄柊寤烘枃浠跺す鈥濆姛鑳斤紝鍙洿鎺ュ湪 Mod 瀛樺偍鐩綍涓嬪垱寤烘柊鐨勫垎绫荤洰褰?
- 绗竴灞傛枃浠跺す鏂板鈥滈噸鍛藉悕鈥濆姛鑳斤紝鍙洿鎺ヤ慨鏀瑰綋鍓嶅垎绫荤洰褰曞悕绉?
- 绗簩灞傚尯鍩熸柊澧炩€滃垹闄ゅ綋鍓?Mod鈥濆姛鑳?
- 鍒犻櫎绗簩灞?Mod 鍓嶄細寮瑰嚭纭绐楀彛锛岄伩鍏嶈鍒?
- 鏄剧ず鐗堟湰鏇存柊涓?`v2.2.5`锛屾枃浠剁増鏈洿鏂颁负 `2.2.5.0`

### English

- Added a `New Folder` action for first-level folders to create new category folders directly inside the mod storage directory
- Added a `Rename` action for first-level folders to rename the current category folder
- Added a `Delete Mod` action to the second-level section
- Added a confirmation dialog before deleting a second-level mod to reduce accidental removal
- Updated the displayed app version to `v2.2.5` and the file version to `2.2.5.0`

## v2.2.4

### 涓枃

- 蹇嵎閿綍鍏ユ柊澧炵鍙烽敭鏀寔锛屼慨澶?`/` 绛夋寜閿樉绀烘垚鏁板瓧閿€肩殑闂
- 蹇嵎閿鏄庢枃妗堝悓姝ユ洿鏂帮紝鏄庣‘鏀寔鍗曢敭銆佺粍鍚堥敭鍜岀鍙烽敭
- 鍘嬬缉鏂囦欢瀵煎叆鍏ュ彛鏀逛负浣滅敤浜庡綋鍓嶇涓€灞傚垎绫荤洰褰曪紝涓嶅啀瀵煎叆鍒板綋鍓嶇浜屽眰 Mod
- 鍘嬬缉鏂囦欢瀵煎叆鎵╁睍涓烘敮鎸佹洿澶氭牸寮忥紝鍖呮嫭 `.zip`銆乣.7z`銆乣.tar`銆乣.gz`銆乣.tgz`銆乣.bz2`銆乣.xz`
- 鏂板绗竴灞傛枃浠跺す鎼滅储妗嗭紝鍙洿鎺ョ瓫閫夊垎绫荤洰褰?
- 搴曢儴浣滆€呮枃妗堟敼涓衡€滃伐鍏蜂綔鑰呪€?
- 鏄剧ず鐗堟湰鏇存柊涓?`v2.2.4`锛屾枃浠剁増鏈洿鏂颁负 `2.2.4.0`

### English

- Added symbol key support for shortcut capture and fixed `/` and similar keys being shown as raw numeric key codes
- Updated the shortcut helper text to clarify support for single keys, key combinations, and symbol keys
- Changed archive import so it now targets the currently selected first-level category folder instead of the second-level mod folder
- Expanded archive import support to more formats, including `.zip`, `.7z`, `.tar`, `.gz`, `.tgz`, `.bz2`, and `.xz`
- Added a search box for first-level folders to filter category entries quickly
- Changed the footer author label to `Tool author`
- Updated the displayed app version to `v2.2.4` and the file version to `2.2.4.0`

## v2.2.3

### 涓枃

- 浼樺寲绗簩灞傚垪琛ㄦ樉绀猴紝绉婚櫎鍚嶇О鍚庨潰鐨勬暟瀛楀垪锛屽彧淇濈暀鏇存竻鏅扮殑鍚嶇О鍜岀姸鎬?
- 绗簩灞傚悕绉板尯鍩熸敮鎸佸崟琛岀渷鐣ユ樉绀猴紝閬垮厤闀垮悕绉版尋鍘嬪竷灞€
- 蹇嵎閿尯鍩熻皟鏁翠负 `1/3` 蹇嵎閿姞 `2/3` 鎻忚堪鐨勬瘮渚嬶紝鏇撮€傚悎褰曞叆鍜屾煡鐪?
- 蹇嵎閿鏄庢枃妗堟洿鏂帮紝鏄庣‘鏀寔鍗曢敭鍜岀粍鍚堥敭褰曞叆
- 椤堕儴璺緞鍖哄懡鍚嶆洿鏂颁负鈥淢od 瀛樺偍鏂囦欢澶光€濆拰鈥滅洰鏍囨枃浠跺す鈥濓紝鐣岄潰琛ㄦ剰鏇存竻鏅?
- 鏄剧ず鐗堟湰鏇存柊涓?`v2.2.3`锛屾枃浠剁増鏈洿鏂颁负 `2.2.3.0`

### English

- Refined the second-level list layout by removing the trailing number column and keeping a cleaner name-and-state view
- Added single-line ellipsis trimming for long second-level mod names to avoid layout crowding
- Adjusted the shortcut section to a `1/3` shortcut field and `2/3` description field layout for better readability
- Updated the shortcut help text to clarify that both single keys and key combinations are supported
- Renamed the top path labels to `Mod Storage` and `Target Folder` for clearer wording
- Updated the displayed app version to `v2.2.3` and the file version to `2.2.3.0`

## v2.2.2

### 涓枃

- 鏂板涓嫳鍒囨崲鍔熻兘锛岀晫闈㈡爣棰樸€佹寜閽€佽鏄庛€佺姸鎬佸拰寮圭獥鏀寔涓嫳鏂囧垏鎹?
- 璋冩暣涓荤晫闈㈠竷灞€锛屽噺灏戞枃瀛楁嫢鎸ゅ拰搴曢儴璇存槑琚鍒囩殑闂
- 鍦ㄥ浘鐗囬瑙堜笅鏂规柊澧?Mod 閾炬帴妯″潡锛屾瘡涓浜屽眰 Mod 閮藉彲浠ョ粦瀹氱嫭绔嬮摼鎺?
- 鏂板蹇€熻闂寜閽紝鍙洿鎺ヨ皟鐢ㄧ郴缁熼粯璁ゆ祻瑙堝櫒鎵撳紑褰撳墠 Mod 瀵瑰簲閾炬帴
- 鍥哄畾鍥剧墖棰勮鍖哄煙楂樺害锛岄伩鍏嶄笉鍚屽浘鐗囧垏鎹㈡椂绐楀彛鍐呭璺冲姩
- 閰嶇疆鏂囦欢鏂板淇濆瓨璇█璁剧疆鍜屾瘡涓?Mod 鐨勯摼鎺ヤ俊鎭?
- 鏄剧ず鐗堟湰鏇存柊涓?`v2.2.2`锛屾枃浠剁増鏈洿鏂颁负 `2.2.2.0`

### English

- Added a Chinese / English language toggle for titles, buttons, hints, status text, and dialogs
- Adjusted the main layout to reduce crowding and prevent helper text from being clipped
- Added a mod link section below the preview area, with a dedicated link for each second-level mod
- Added a quick access button that opens the current mod link with the system default browser
- Fixed the preview area height so the layout no longer jumps when switching between images
- The config file now stores both the selected language and per-mod links
- Updated the app version to `v2.2.2` and the file version to `2.2.2.0`
