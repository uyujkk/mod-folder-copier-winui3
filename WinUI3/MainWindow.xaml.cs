using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using WinRT.Interop;

namespace ModFolderCopier.WinUI;

public sealed partial class MainWindow : Window
{
    private const string AppVersion = "v2.2.8";
    private const int MaxShortcutRows = 10;
    private static readonly string[] SupportedArchiveExtensions = [".zip", ".7z", ".tar", ".gz", ".tgz", ".bz2", ".xz"];

    private enum AppLanguage
    {
        ZhCn,
        EnUs
    }

    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"];
    private static readonly SolidColorBrush CopiedBrush = new(ColorHelper.FromArgb(255, 36, 124, 76));
    private static readonly SolidColorBrush MissingBrush = new(ColorHelper.FromArgb(255, 177, 76, 55));
    private static readonly SolidColorBrush NeutralBrush = new(ColorHelper.FromArgb(255, 60, 60, 60));

    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");
    private readonly ObservableCollection<FirstLevelFolderItem> _firstLevelItems = [];
    private readonly ObservableCollection<SecondLevelFolderItem> _secondLevelItems = [];
    private readonly List<FirstLevelFolderItem> _allFirstLevelItems = [];
    private readonly List<Border> _shortcutKeyBorders = [];
    private readonly List<TextBox> _shortcutKeyBoxes = [];
    private readonly List<TextBox> _shortcutActionBoxes = [];
    private readonly Dictionary<string, List<ShortcutBinding>> _modBindings = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly Dictionary<string, string> _modLinks = new(StringComparer.CurrentCultureIgnoreCase);

    private bool _isDarkTheme;
    private bool _isLoadingBindings;
    private bool _isLoadingModLink;
    private int _visibleShortcutRows = 1;
    private TextBox? _activeShortcutKeyBox;
    private string? _currentSecondLevelPath;
    private AppLanguage _currentLanguage = AppLanguage.ZhCn;

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = false;
        FirstLevelListView.ItemsSource = _firstLevelItems;
        SecondLevelListView.ItemsSource = _secondLevelItems;
        BuildShortcutRows();
        Activated += (_, _) => RootGrid.Focus(FocusState.Programmatic);
        LoadConfig();
        ApplyTheme(_isDarkTheme);
        ApplyLanguage();

        if (Directory.Exists(SourceTextBox.Text))
        {
            _ = RefreshListsAsync();
        }
        else
        {
            SetDefaultStatus();
        }
    }

    private string L(string zh, string en) => _currentLanguage == AppLanguage.EnUs ? en : zh;

    private string StateNotSelectedText => L("未选择", "Not selected");

    private string StateNotConfiguredText => L("未设置", "Not configured");

    private string StateCopiedText => L("已复制", "Copied");

    private string StateMissingText => L("未复制", "Not copied");

    private void ApplyLanguage()
    {
        Title = L($"Mod 文件复制器 {AppVersion}", $"Mod Folder Copier {AppVersion}");

        HeaderTitleTextBlock.Text = L("Mod 文件复制器", "Mod Folder Copier");
        HeaderFrameworkBadgeTextBlock.Text = "WinUI 3";
        HeaderVersionBadgeTextBlock.Text = AppVersion;
        HeaderSubtitleTextBlock.Text = L(
            "管理两层 Mod 文件夹、预览图、ZIP 导入、快捷键说明和启动器入口。",
            "Manage two-level mod folders, preview images, ZIP imports, per-mod shortcut notes, and launcher access.");
        HeaderCaptionTextBlock.Text = L(
            "左侧先选第一层分类，再选第二层 Mod。双击第二层可直接复制或移除。",
            "Select a first-level category first, then a second-level mod. Double-click a mod to copy or remove it.");

        LanguageToggleButton.Content = _currentLanguage == AppLanguage.ZhCn ? "English" : "中文";
        ThemeToggleButton.Content = _isDarkTheme ? L("切换浅色", "Light theme") : L("切换深色", "Dark theme");

        PathSectionTitleTextBlock.Text = L("路径与启动器", "Paths and Launcher");
        PathSectionSubtitleTextBlock.Text = L(
            "把主文件夹、副文件夹和启动器统一放在这里，常用操作会直接使用这些路径。",
            "Keep the source folder, target folder, and launcher together here for quick access.");
        SourceLabelTextBlock.Text = L("主文件夹", "Source");
        TargetLabelTextBlock.Text = L("副文件夹", "Target");
        LauncherLabelTextBlock.Text = L("启动器", "Launcher");
        PickSourceButton.Content = L("选择", "Browse");
        PickTargetButton.Content = L("选择", "Browse");
        PickLauncherButton.Content = L("选择程序", "Browse EXE");
        OpenSourceButton.Content = L("打开主文件夹", "Open Source");
        OpenTargetButton.Content = L("打开副文件夹", "Open Target");
        OpenLauncherButton.Content = L("打开启动器位置", "Open Launcher Folder");
        LauncherTextBox.PlaceholderText = L("选择 XXMI Launcher.exe 或其他启动器", "Select XXMI Launcher.exe or another launcher");
        PathHintTextBlock.Text = L(
            "支持刷新目录、导入 ZIP、复制当前第二层文件夹，以及直接运行外部启动器。",
            "Refresh folders, import ZIP files, copy the selected second-level folder, or run the external launcher.");
        RefreshButton.Content = L("刷新目录", "Refresh");
        ImportZipButton.Content = L("导入到当前选中文件夹", "Import To Selected Folder");
        RunLauncherButton.Content = L("运行启动器", "Run Launcher");
        ToggleCopyButton.Content = L("复制当前第二层文件夹", "Copy Selected Mod");

        FirstCountLabelTextBlock.Text = L("第一层目录数", "First-level folders");
        FirstCountHintTextBlock.Text = L("当前主文件夹下的分类数量", "Number of categories in the source folder");
        SecondCountLabelTextBlock.Text = L("第二层目录数", "Second-level folders");
        SecondCountHintTextBlock.Text = L("当前扫描到的 Mod 总数", "Total mod folders found in the current source");
        CurrentStateLabelTextBlock.Text = L("当前复制状态", "Copy status");
        CurrentStateHintTextBlock.Text = L("根据副文件夹中的同名目录判断", "Detected from folders with the same name in the target");
        CurrentFolderLabelTextBlock.Text = L("当前第二层文件夹", "Current second-level folder");
        CurrentFolderHintTextBlock.Text = L("这里会显示当前选中的 Mod 名称", "Shows the currently selected mod name");

        FirstLevelSectionTitleTextBlock.Text = L("第一层文件夹", "First-level folders");
        FirstLevelSectionSubtitleTextBlock.Text = L("先选择分类目录", "Choose a category folder first");
        FirstLevelSearchTextBox.PlaceholderText = L("搜索第一层文件夹", "Search first-level folders");
        CreateFirstLevelButton.Content = L("新建第一层文件夹", "New Folder");
        RenameFirstLevelButton.Content = L("重命名当前第一层", "Rename");
        SecondLevelSectionTitleTextBlock.Text = L("第二层文件夹", "Second-level folders");
        SecondLevelSectionSubtitleTextBlock.Text = L("再选择具体 Mod", "Then choose a specific mod");
        DeleteSecondLevelButton.Content = L("删除当前第二层 Mod", "Delete Mod");

        ShortcutSectionTitleTextBlock.Text = L("快捷键与功能", "Shortcut Notes");
        ShortcutSectionSubtitleTextBlock.Text = L("当前选中 Mod 的快捷键说明", "Shortcut notes for the selected mod");
        AddShortcutRowButton.Content = L("新增一行", "Add Row");
        ShortcutHintTextBlock.Text = L(
            "快捷键需要至少两个按键组合。先点选快捷键输入框，再按下组合键即可自动录入；当前窗口聚焦时，会定位并执行对应 Mod。",
            "Shortcuts must use at least a two-key combination. Click a shortcut box first, then press the combination to capture it; when this window is focused, it will locate and run the corresponding mod.");

        PreviewSectionTitleTextBlock.Text = L("默认图像预览", "Image Preview");
        PreviewSectionSubtitleTextBlock.Text = L(
            "优先 preview / cover / thumbnail / image，也支持把图片拖到这里。",
            "Prefers preview / cover / thumbnail / image, and also supports dragging an image here.");
        ModLinkSectionTitleTextBlock.Text = L("Mod 链接", "Mod Link");
        ModLinkSectionSubtitleTextBlock.Text = L(
            "为当前 Mod 绑定一个网页链接，可直接快速访问。",
            "Bind a web link to the current mod for quick access.");
        ModLinkTextBox.PlaceholderText = L("粘贴当前 Mod 对应的网址", "Paste the web link for the current mod");
        PreviewFooterTextBlock.Text = L(
            "预览区会显示当前第二层文件夹的图片，下方链接可一键用默认浏览器打开。",
            "The preview shows the current second-level folder image, and the link below opens in the default browser.");
        OpenModLinkButton.Content = L("快速访问", "Open Link");

        ProgressTextBlock.Text = string.IsNullOrWhiteSpace(ProgressTextBlock.Text)
            ? L("当前无复制任务", "No active copy task")
            : ProgressTextBlock.Text;

        AuthorTextBlock.Text = L("工具作者：uyujkk", "Tool author: uyujkk");

        ApplyShortcutPlaceholders();
        UpdateShortcutRowVisibility();
        UpdateLinkButtonState();
        RefreshLocalizedStates();
        ApplyTextOverrides();
        ApplyActionButtonPresentation();

        if (FirstLevelListView.SelectedItem is FirstLevelFolderItem firstItem)
        {
            string? selectedPath = _currentSecondLevelPath;
            PopulateSecondLevelList(firstItem, preserveStatus: true);
            SelectSecondLevelByPath(selectedPath);
        }
        else if (GetSelectedSecondLevelItem() is SecondLevelFolderItem secondItem)
        {
            ShowSecondLevelDetails(secondItem, preserveStatus: true);
        }
        else
        {
            CurrentFolderTextBlock.Text = StateNotSelectedText;
            CurrentStateTextBlock.Text = StateNotSelectedText;
            SetStateColor(CurrentStateTextBlock.Text);
            ClearPreview();
            SetDefaultStatus();
        }
    }

    private void ApplyTextOverrides()
    {
        PathSectionSubtitleTextBlock.Text = L(
            "把 Mod 存储文件夹、目标文件夹和启动器统一放在这里，常用操作可以直接使用这些路径。",
            "Keep the mod storage folder, target folder, and launcher together here for quick access.");
        SourceLabelTextBlock.Text = L("Mod 存储文件夹", "Mod Storage");
        TargetLabelTextBlock.Text = L("目标文件夹", "Target Folder");
        OpenSourceButton.Content = L("打开 Mod 存储文件夹", "Open Mod Storage");
        OpenTargetButton.Content = L("打开目标文件夹", "Open Target Folder");
        PathHintTextBlock.Text = L(
            "可以刷新目录、导入 ZIP、将当前第二层 Mod 复制到目标文件夹，也可以直接运行外部启动器。",
            "Refresh folders, import ZIP files, copy the selected second-level mod into the target folder, or run the external launcher.");
        ImportZipButton.Content = L("导入到当前选中文件夹", "Import To Selected Folder");

        FirstCountHintTextBlock.Text = L("当前 Mod 存储文件夹下的分类数量", "Number of categories in the mod storage folder");

        ShortcutSectionTitleTextBlock.Text = L("快捷键与描述", "Shortcut and Description");
        ShortcutSectionSubtitleTextBlock.Text = L("为当前选中的 Mod 记录快捷键和描述", "Record a shortcut and description for the selected mod");
        ShortcutHintTextBlock.Text = L(
            "点击快捷键输入框后可直接按键录入，支持单键、组合键和符号键；当前窗口聚焦时，按已绑定的快捷键会定位并执行对应 Mod。",
            "Click a shortcut box and press a key to capture it. Single keys, key combinations, and symbol keys are supported; when this window is focused, the bound shortcut will locate and run the corresponding mod.");
    }

    private void ApplyActionButtonPresentation()
    {
        CreateFirstLevelButton.Content = new FontIcon { Glyph = "\uE710", FontSize = 16 };
        RenameFirstLevelButton.Content = new FontIcon { Glyph = "\uE70F", FontSize = 16 };
        DeleteSecondLevelButton.Content = new FontIcon { Glyph = "\uE74D", FontSize = 16 };

        string createTip = L("新建第一层文件夹", "Create first-level folder");
        string renameTip = L("重命名当前第一层", "Rename first-level folder");
        string deleteTip = L("删除当前第二层 Mod", "Delete selected mod");

        ToolTipService.SetToolTip(CreateFirstLevelButton, createTip);
        ToolTipService.SetToolTip(RenameFirstLevelButton, renameTip);
        ToolTipService.SetToolTip(DeleteSecondLevelButton, deleteTip);

        AutomationProperties.SetName(CreateFirstLevelButton, createTip);
        AutomationProperties.SetName(RenameFirstLevelButton, renameTip);
        AutomationProperties.SetName(DeleteSecondLevelButton, deleteTip);
    }

    private void ApplyShortcutPlaceholders()
    {
        foreach (TextBox box in _shortcutKeyBoxes)
        {
            box.PlaceholderText = L("快捷键", "Shortcut");
        }

        foreach (TextBox box in _shortcutActionBoxes)
        {
            box.PlaceholderText = L("描述", "Description");
        }
    }

    private void SetDefaultStatus()
    {
        StatusTextBlock.Text = L(
            $"请选择 Mod 存储文件夹和目标文件夹。当前版本：{AppVersion}",
            $"Choose a mod storage folder and target folder. Current version: {AppVersion}");
    }

    private void BuildShortcutRows()
    {
        ShortcutRowsPanel.Children.Clear();
        _shortcutKeyBorders.Clear();
        _shortcutKeyBoxes.Clear();
        _shortcutActionBoxes.Clear();

        for (int index = 0; index < MaxShortcutRows; index++)
        {
            var rowGrid = new Grid { ColumnSpacing = 12 };
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            var keyBorder = CreateInsetBorder();
            var keyBox = new TextBox
            {
                IsReadOnly = true,
                Style = (Style)Application.Current.Resources["ShortcutInputTextBoxStyle"]
            };
            ConfigureShortcutTextBoxTheme(keyBox);
            keyBox.TextChanged += OnShortcutTextChanged;
            keyBox.KeyDown += OnShortcutKeyBoxKeyDown;
            keyBox.GotFocus += OnShortcutKeyBoxGotFocus;
            keyBox.LostFocus += OnShortcutKeyBoxLostFocus;
            keyBorder.Child = keyBox;

            var actionBorder = CreateInsetBorder();
            var actionBox = new TextBox
            {
                Style = (Style)Application.Current.Resources["ShortcutInputTextBoxStyle"]
            };
            ConfigureShortcutTextBoxTheme(actionBox);
            actionBox.TextChanged += OnShortcutTextChanged;
            actionBorder.Child = actionBox;

            Grid.SetColumn(keyBorder, 0);
            Grid.SetColumn(actionBorder, 1);
            rowGrid.Children.Add(keyBorder);
            rowGrid.Children.Add(actionBorder);

            ShortcutRowsPanel.Children.Add(rowGrid);
            _shortcutKeyBorders.Add(keyBorder);
            _shortcutKeyBoxes.Add(keyBox);
            _shortcutActionBoxes.Add(actionBox);
        }

        ApplyShortcutPlaceholders();
        UpdateShortcutRowVisibility();
        UpdateShortcutKeyFocusVisuals();
    }

    private static Border CreateInsetBorder()
    {
        return new Border
        {
            Style = (Style)Application.Current.Resources["InsetBorderStyle"]
        };
    }

    private static void ConfigureShortcutTextBoxTheme(TextBox box)
    {
        var transparentBrush = new SolidColorBrush(Colors.Transparent);
        box.Resources["TextControlBackground"] = transparentBrush;
        box.Resources["TextControlBackgroundFocused"] = transparentBrush;
        box.Resources["TextControlBackgroundPointerOver"] = transparentBrush;
        box.Resources["TextControlBackgroundDisabled"] = transparentBrush;
        box.Resources["TextControlBackgroundReadOnly"] = transparentBrush;
        box.Resources["TextControlBorderBrush"] = transparentBrush;
        box.Resources["TextControlBorderBrushFocused"] = transparentBrush;
        box.Resources["TextControlBorderBrushPointerOver"] = transparentBrush;
        box.Resources["TextControlBorderBrushDisabled"] = transparentBrush;
        box.Resources["TextControlBorderBrushReadOnly"] = transparentBrush;
        box.Resources["TextControlForegroundReadOnly"] = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
    }

    private void OnShortcutKeyBoxGotFocus(object sender, RoutedEventArgs e)
    {
        _activeShortcutKeyBox = sender as TextBox;
        UpdateShortcutKeyFocusVisuals();
    }

    private void OnShortcutKeyBoxLostFocus(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(_activeShortcutKeyBox, sender))
        {
            _activeShortcutKeyBox = null;
        }

        UpdateShortcutKeyFocusVisuals();
    }

    private void UpdateShortcutKeyFocusVisuals()
    {
        Brush normalBackground = (Brush)Application.Current.Resources["AppInsetBackgroundBrush"];
        Brush normalBorder = (Brush)Application.Current.Resources["AppCardBorderBrush"];
        Brush activeBackground = (Brush)Application.Current.Resources["AppAccentSoftBrush"];
        Brush activeBorder = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        object? focusedElement = Content is FrameworkElement root && root.XamlRoot is not null
            ? FocusManager.GetFocusedElement(root.XamlRoot)
            : null;

        for (int i = 0; i < _shortcutKeyBorders.Count; i++)
        {
            bool isActive = i < _shortcutKeyBoxes.Count &&
                ReferenceEquals(_shortcutKeyBoxes[i], _activeShortcutKeyBox) &&
                ReferenceEquals(_shortcutKeyBoxes[i], focusedElement);
            _shortcutKeyBorders[i].Background = isActive ? activeBackground : normalBackground;
            _shortcutKeyBorders[i].BorderBrush = isActive ? activeBorder : normalBorder;
            _shortcutKeyBorders[i].BorderThickness = isActive ? new Thickness(2) : new Thickness(1);
        }
    }


    private async void OnPickSourceClicked(object sender, RoutedEventArgs e)
    {
        string? picked = await PickFolderAsync();
        if (!string.IsNullOrEmpty(picked))
        {
            SourceTextBox.Text = picked;
            SaveConfig();
            await RefreshListsAsync();
        }
    }

    private async void OnPickTargetClicked(object sender, RoutedEventArgs e)
    {
        string? picked = await PickFolderAsync();
        if (!string.IsNullOrEmpty(picked))
        {
            TargetTextBox.Text = picked;
            SaveConfig();
            await RefreshListsAsync();
        }
    }

    private async void OnPickLauncherClicked(object sender, RoutedEventArgs e)
    {
        string? picked = await PickFileAsync([".exe", ".bat", ".cmd"]);
        if (!string.IsNullOrEmpty(picked))
        {
            LauncherTextBox.Text = picked;
            SaveConfig();
        }
    }

    private void OnOpenSourceClicked(object sender, RoutedEventArgs e) => OpenDirectory(SourceTextBox.Text, L("主文件夹", "source folder"));

    private void OnOpenTargetClicked(object sender, RoutedEventArgs e) => OpenDirectory(TargetTextBox.Text, L("副文件夹", "target folder"));

    private void OnOpenLauncherClicked(object sender, RoutedEventArgs e) => OpenFileLocation(LauncherTextBox.Text, L("启动器", "launcher"));

    private async void OnRunLauncherClicked(object sender, RoutedEventArgs e)
    {
        string launcherPath = (LauncherTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(launcherPath) || !File.Exists(launcherPath))
        {
            await ShowMessageAsync(
                L("请先设置有效的 XXMI 启动器路径。", "Set a valid launcher path first."),
                L("未设置启动器", "Launcher not set"));
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = launcherPath,
                WorkingDirectory = Path.GetDirectoryName(launcherPath) ?? AppContext.BaseDirectory,
                UseShellExecute = true
            });
            StatusTextBlock.Text = L("已启动外部启动器。", "External launcher started.");
            SaveConfig();
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("运行启动器失败：", "Failed to run launcher: ") + ex.Message,
                L("启动失败", "Launch failed"));
        }
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e) => await RefreshListsAsync();

    private async void OnImportZipClicked(object sender, RoutedEventArgs e)
    {
        FirstLevelFolderItem? item = FirstLevelListView.SelectedItem as FirstLevelFolderItem;
        if (item is null)
        {
            await ShowMessageAsync(
                L("请先选择一个第一层分类。", "Select a first-level category first."),
                L("未选择分类", "No category selected"));
            return;
        }

        var picker = new FileOpenPicker();
        foreach (string extension in SupportedArchiveExtensions)
        {
            picker.FileTypeFilter.Add(extension);
        }
        picker.SuggestedStartLocation = PickerLocationId.Downloads;
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        StorageFile? file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            await ImportArchiveToSelectedFirstLevelFolderAsync(file.Path, item);
        }
    }

    private async void OnToggleCopyClicked(object sender, RoutedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        if (item is null)
        {
            await ShowMessageAsync(
                L("请先选择一个第二层 Mod。", "Select a second-level mod first."),
                L("未选择 Mod", "No mod selected"));
            return;
        }

        await ToggleDirectoryCopyAsync(item);
    }

    private void OnFirstLevelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PopulateSecondLevelList(FirstLevelListView.SelectedItem as FirstLevelFolderItem);
    }

    private void OnFirstLevelSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFirstLevelFilter();
    }

    private async void OnCreateFirstLevelClicked(object sender, RoutedEventArgs e)
    {
        string sourceDir = (SourceTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
        {
            await ShowMessageAsync(
                L("请先设置有效的 Mod 存储文件夹。", "Set a valid mod storage folder first."),
                L("路径无效", "Invalid path"));
            return;
        }

        string? name = await PromptForTextAsync(
            L("请输入新的第一层文件夹名称。", "Enter the new first-level folder name."),
            L("新建第一层文件夹", "Create first-level folder"),
            string.Empty);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        try
        {
            string targetPath = Path.Combine(sourceDir, name.Trim());
            if (Directory.Exists(targetPath))
            {
                await ShowMessageAsync(
                    L("同名第一层文件夹已存在。", "A first-level folder with the same name already exists."),
                    L("创建失败", "Create failed"));
                return;
            }

            Directory.CreateDirectory(targetPath);
            await RefreshListsAsync();
            SelectFirstLevelByPath(targetPath);
            StatusTextBlock.Text = L($"已创建第一层文件夹：{name.Trim()}", $"Created first-level folder: {name.Trim()}");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("创建第一层文件夹失败：", "Failed to create the first-level folder: ") + ex.Message,
                L("创建失败", "Create failed"));
        }
    }

    private async void OnRenameFirstLevelClicked(object sender, RoutedEventArgs e)
    {
        FirstLevelFolderItem? item = FirstLevelListView.SelectedItem as FirstLevelFolderItem;
        if (item is null)
        {
            await ShowMessageAsync(
                L("请先选择一个第一层文件夹。", "Select a first-level folder first."),
                L("未选择文件夹", "No folder selected"));
            return;
        }

        string? name = await PromptForTextAsync(
            L("请输入新的第一层文件夹名称。", "Enter the new first-level folder name."),
            L("重命名第一层文件夹", "Rename first-level folder"),
            item.Name);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        string trimmed = name.Trim();
        if (string.Equals(trimmed, item.Name, StringComparison.CurrentCulture))
        {
            return;
        }

        try
        {
            string? parentDir = Path.GetDirectoryName(item.Path);
            if (string.IsNullOrWhiteSpace(parentDir))
            {
                throw new InvalidOperationException(L("无法确定父目录。", "Could not determine the parent directory."));
            }

            string newPath = Path.Combine(parentDir, trimmed);
            if (Directory.Exists(newPath))
            {
                await ShowMessageAsync(
                    L("目标名称已存在，请换一个名称。", "The target name already exists. Choose a different name."),
                    L("重命名失败", "Rename failed"));
                return;
            }

            Directory.Move(item.Path, newPath);
            await RefreshListsAsync();
            SelectFirstLevelByPath(newPath);
            StatusTextBlock.Text = L($"已重命名第一层文件夹为：{trimmed}", $"Renamed first-level folder to: {trimmed}");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("重命名第一层文件夹失败：", "Failed to rename the first-level folder: ") + ex.Message,
                L("重命名失败", "Rename failed"));
        }
    }

    private void OnSecondLevelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        _currentSecondLevelPath = item?.Path;
        ShowSecondLevelDetails(item);
        LoadBindingsForCurrentMod(item);
        LoadLinkForCurrentMod(item);
    }

    private async void OnSecondLevelDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        if (item is not null)
        {
            await ToggleDirectoryCopyAsync(item);
        }
    }

    private async void OnDeleteSecondLevelClicked(object sender, RoutedEventArgs e)
    {
        SecondLevelFolderItem? item = GetSelectedSecondLevelItem();
        if (item is null)
        {
            await ShowMessageAsync(
                L("请先选择一个第二层 Mod。", "Select a second-level mod first."),
                L("未选择 Mod", "No mod selected"));
            return;
        }

        bool confirmed = await ShowConfirmAsync(
            L($"确定要删除这个第二层 Mod 吗？\n\n{item.Name}\n\n此操作会删除原始文件夹，无法撤销。",
              $"Are you sure you want to delete this second-level mod?\n\n{item.Name}\n\nThis will delete the original folder and cannot be undone."),
            L("确认删除", "Confirm delete"));
        if (!confirmed)
        {
            return;
        }

        try
        {
            string selectedFirstLevelPath = (FirstLevelListView.SelectedItem as FirstLevelFolderItem)?.Path ?? string.Empty;
            Directory.Delete(item.Path, true);
            await RefreshListsAsync();
            SelectFirstLevelByPath(selectedFirstLevelPath);
            StatusTextBlock.Text = L($"已删除第二层 Mod：{item.Name}", $"Deleted second-level mod: {item.Name}");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("删除第二层 Mod 失败：", "Failed to delete the second-level mod: ") + ex.Message,
                L("删除失败", "Delete failed"));
        }
    }

    private async void OnAddShortcutRowClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentSecondLevelPath))
        {
            await ShowMessageAsync(
                L("请先选择一个第二层 Mod。", "Select a second-level mod first."),
                L("未选择 Mod", "No mod selected"));
            return;
        }

        if (_visibleShortcutRows >= MaxShortcutRows)
        {
            StatusTextBlock.Text = L("快捷键行数已达到上限 10 行。", "Shortcut rows have reached the 10-row limit.");
            return;
        }

        _visibleShortcutRows++;
        UpdateShortcutRowVisibility();
        _shortcutKeyBoxes[_visibleShortcutRows - 1].Focus(FocusState.Programmatic);
        SaveCurrentModBindings();
    }

    private void OnShortcutTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isLoadingBindings)
        {
            SaveCurrentModBindings();
        }
    }

    private void OnShortcutKeyBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not TextBox box)
        {
            return;
        }

        if (e.KeyStatus.WasKeyDown || e.KeyStatus.RepeatCount > 1)
        {
            e.Handled = true;
            return;
        }

        if (e.Key is VirtualKey.Back or VirtualKey.Delete or VirtualKey.Escape)
        {
            box.Text = string.Empty;
            e.Handled = true;
            return;
        }

        string shortcut = BuildShortcutFromEvent(e);
        if (string.IsNullOrEmpty(shortcut))
        {
            return;
        }

        box.Text = shortcut;
        box.SelectionStart = box.Text.Length;
        e.Handled = true;
    }

    private void OnModLinkTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isLoadingModLink)
        {
            SaveCurrentModLink();
        }
    }

    private async void OnOpenModLinkClicked(object sender, RoutedEventArgs e)
    {
        string currentLink = (ModLinkTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(currentLink))
        {
            await ShowMessageAsync(
                L("请先为当前 Mod 填写一个可访问的网址。", "Enter a valid web link for the current mod first."),
                L("未设置链接", "Link not set"));
            return;
        }

        try
        {
            string normalized = NormalizeLink(currentLink);
            Process.Start(new ProcessStartInfo
            {
                FileName = normalized,
                UseShellExecute = true
            });
            StatusTextBlock.Text = L("已在默认浏览器中打开 Mod 链接。", "Opened the mod link in the default browser.");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("打开链接失败：", "Failed to open link: ") + ex.Message,
                L("打开失败", "Open failed"));
        }
    }

    private void OnThemeToggleClicked(object sender, RoutedEventArgs e)
    {
        ApplyTheme(!_isDarkTheme);
        SaveConfig();
    }

    private void OnLanguageToggleClicked(object sender, RoutedEventArgs e)
    {
        _currentLanguage = _currentLanguage == AppLanguage.ZhCn ? AppLanguage.EnUs : AppLanguage.ZhCn;
        ApplyLanguage();
        SaveConfig();
    }

    private void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.None;

        if (GetSelectedSecondLevelItem() is null)
        {
            return;
        }

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
    }

    private async void OnPreviewDrop(object sender, DragEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        if (item is null)
        {
            await ShowMessageAsync(
                L("请先选择一个第二层 Mod，再把图片拖到预览区。", "Select a second-level mod before dropping an image."),
                L("未选择 Mod", "No mod selected"));
            return;
        }

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            await ShowMessageAsync(
                L("只支持从资源管理器拖入图片文件。", "Only image files dragged from File Explorer are supported."),
                L("不支持的拖放内容", "Unsupported drop content"));
            return;
        }

        IReadOnlyList<IStorageItem> droppedItems = await e.DataView.GetStorageItemsAsync();
        StorageFile? imageFile = droppedItems.OfType<StorageFile>().FirstOrDefault(file => IsImageFile(file.Path));
        if (imageFile is null)
        {
            await ShowMessageAsync(
                L("没有检测到可用的图片文件。", "No supported image file was detected."),
                L("未找到图片", "No image found"));
            return;
        }

        await ImportPreviewImageAsync(imageFile.Path, item);
    }

    private void OnSecondLevelDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.None;

        if (FirstLevelListView.SelectedItem is null)
        {
            return;
        }

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
    }

    private async void OnSecondLevelDrop(object sender, DragEventArgs e)
    {
        FirstLevelFolderItem? item = FirstLevelListView.SelectedItem as FirstLevelFolderItem;
        if (item is null)
        {
            await ShowMessageAsync(
                L("璇峰厛閫夋嫨涓€涓涓€灞傚垎绫伙紝鍐嶆妸鍘嬬缉鍖呮嫋鍒拌繖閲屻€?", "Select a first-level category before dropping an archive here."),
                L("鏈€夋嫨鍒嗙被", "No category selected"));
            return;
        }

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            await ShowMessageAsync(
                L("鍙敮鎸佷粠璧勬簮绠＄悊鍣ㄦ嫋鍏ュ帇缂╂枃浠躲€?", "Only archive files dragged from File Explorer are supported here."),
                L("涓嶆敮鎸佺殑鎷栨斁鍐呭", "Unsupported drop content"));
            return;
        }

        IReadOnlyList<IStorageItem> droppedItems = await e.DataView.GetStorageItemsAsync();
        StorageFile? archiveFile = droppedItems
            .OfType<StorageFile>()
            .FirstOrDefault(file => IsSupportedArchiveFile(file.Path));

        if (archiveFile is null)
        {
            await ShowMessageAsync(
                L("娌℃湁妫€娴嬪埌鏀寔鐨勫帇缂╂枃浠躲€?", "No supported archive file was detected."),
                L("鏈壘鍒板帇缂╂枃浠?", "No archive found"));
            return;
        }

        await ImportArchiveToSecondLevelFolderAsync(archiveFile.Path, item);
    }

    private async void OnRootKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (FocusManager.GetFocusedElement(this.Content.XamlRoot) is TextBox)
        {
            return;
        }

        if (e.Key == VirtualKey.F5)
        {
            await RefreshListsAsync();
            e.Handled = true;
            return;
        }

        if (e.Key == VirtualKey.Enter)
        {
            var item = GetSelectedSecondLevelItem();
            if (item is not null)
            {
                await ToggleDirectoryCopyAsync(item);
                e.Handled = true;
                return;
            }
        }

        string shortcut = BuildShortcutFromEvent(e);
        if (!string.IsNullOrEmpty(shortcut) && await TryRunBoundShortcutAsync(shortcut))
        {
            e.Handled = true;
        }
    }

    private void OnRootGridPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (IsPointerInsideTextInput(e.OriginalSource as DependencyObject))
        {
            return;
        }

        RootGrid.Focus(FocusState.Programmatic);
    }

    private static bool IsPointerInsideTextInput(DependencyObject? source)
    {
        DependencyObject? current = source;
        while (current is not null)
        {
            if (current is TextBox)
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private void ApplyTheme(bool dark)
    {
        _isDarkTheme = dark;
        RootGrid.RequestedTheme = dark ? ElementTheme.Dark : ElementTheme.Light;
        ThemeToggleButton.Content = dark ? L("切换浅色", "Light theme") : L("切换深色", "Dark theme");
        SetStateColor(CurrentStateTextBlock.Text);
        UpdateShortcutKeyFocusVisuals();
    }

    private async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        picker.SuggestedStartLocation = PickerLocationId.Desktop;
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        StorageFolder? folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    private async Task<string?> PickFileAsync(string[] extensions)
    {
        var picker = new FileOpenPicker();
        picker.SuggestedStartLocation = PickerLocationId.Desktop;
        foreach (string extension in extensions)
        {
            picker.FileTypeFilter.Add(extension);
        }

        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        StorageFile? file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    private void OpenDirectory(string? directory, string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                _ = ShowMessageAsync(
                    title + L("不存在，请重新选择。", " does not exist. Please choose it again."),
                    L("路径无效", "Invalid path"));
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _ = ShowMessageAsync(
                L("打开目录失败：", "Failed to open folder: ") + ex.Message,
                L("操作失败", "Operation failed"));
        }
    }

    private void OpenFileLocation(string? filePath, string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                _ = ShowMessageAsync(
                    title + L("不存在，请重新选择。", " does not exist. Please choose it again."),
                    L("路径无效", "Invalid path"));
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "/select,\"" + filePath + "\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _ = ShowMessageAsync(
                L("打开启动器位置失败：", "Failed to open file location: ") + ex.Message,
                L("操作失败", "Operation failed"));
        }
    }

    private void LoadConfig()
    {
        _modBindings.Clear();
        _modLinks.Clear();

        if (!File.Exists(_configPath))
        {
            LoadDefaultShortcutTemplate();
            return;
        }

        try
        {
            foreach (string line in File.ReadAllLines(_configPath))
            {
                if (line.StartsWith("source_dir=", StringComparison.Ordinal))
                {
                    SourceTextBox.Text = DecodeValue(line["source_dir=".Length..]);
                }
                else if (line.StartsWith("target_dir=", StringComparison.Ordinal))
                {
                    TargetTextBox.Text = DecodeValue(line["target_dir=".Length..]);
                }
                else if (line.StartsWith("launcher_path=", StringComparison.Ordinal))
                {
                    LauncherTextBox.Text = DecodeValue(line["launcher_path=".Length..]);
                }
                else if (line.StartsWith("theme=", StringComparison.Ordinal))
                {
                    _isDarkTheme = string.Equals(DecodeValue(line["theme=".Length..]), "dark", StringComparison.OrdinalIgnoreCase);
                }
                else if (line.StartsWith("language=", StringComparison.Ordinal))
                {
                    string language = DecodeValue(line["language=".Length..]);
                    _currentLanguage = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase)
                        ? AppLanguage.EnUs
                        : AppLanguage.ZhCn;
                }
                else if (line.StartsWith("binding=", StringComparison.Ordinal))
                {
                    string[] parts = line["binding=".Length..].Split('\t');
                    if (parts.Length == 3)
                    {
                        string modPath = DecodeValue(parts[0]);
                        if (!_modBindings.TryGetValue(modPath, out List<ShortcutBinding>? bindings))
                        {
                            bindings = [];
                            _modBindings[modPath] = bindings;
                        }

                        bindings.Add(new ShortcutBinding(DecodeValue(parts[1]), DecodeValue(parts[2])));
                    }
                }
                else if (line.StartsWith("mod_link=", StringComparison.Ordinal))
                {
                    string[] parts = line["mod_link=".Length..].Split('\t');
                    if (parts.Length == 2)
                    {
                        _modLinks[DecodeValue(parts[0])] = DecodeValue(parts[1]);
                    }
                }
            }
        }
        catch
        {
            StatusTextBlock.Text = L("配置读取失败，已忽略旧配置。", "Failed to load config. Older config values were ignored.");
        }

        LoadDefaultShortcutTemplate();
        LoadLinkForCurrentMod(null);
    }

    private void SaveConfig()
    {
        try
        {
            var lines = new List<string>
            {
                "source_dir=" + EncodeValue(SourceTextBox.Text ?? string.Empty),
                "target_dir=" + EncodeValue(TargetTextBox.Text ?? string.Empty),
                "launcher_path=" + EncodeValue(LauncherTextBox.Text ?? string.Empty),
                "theme=" + EncodeValue(_isDarkTheme ? "dark" : "light"),
                "language=" + EncodeValue(_currentLanguage == AppLanguage.EnUs ? "en" : "zh")
            };

            foreach ((string path, List<ShortcutBinding> bindings) in _modBindings.OrderBy(item => item.Key, StringComparer.CurrentCultureIgnoreCase))
            {
                foreach (ShortcutBinding binding in bindings)
                {
                    if (string.IsNullOrWhiteSpace(binding.Shortcut) && string.IsNullOrWhiteSpace(binding.Action))
                    {
                        continue;
                    }

                    lines.Add("binding=" + EncodeValue(path) + "\t" + EncodeValue(binding.Shortcut) + "\t" + EncodeValue(binding.Action));
                }
            }

            foreach ((string path, string link) in _modLinks.OrderBy(item => item.Key, StringComparer.CurrentCultureIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(link))
                {
                    lines.Add("mod_link=" + EncodeValue(path) + "\t" + EncodeValue(link));
                }
            }

            File.WriteAllLines(_configPath, lines);
        }
        catch
        {
            StatusTextBlock.Text = L("配置保存失败，但不影响当前使用。", "Failed to save config, but the app can continue.");
        }
    }

    private static string EncodeValue(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static string DecodeValue(string value)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task RefreshListsAsync()
    {
        _firstLevelItems.Clear();
        _secondLevelItems.Clear();
        _currentSecondLevelPath = null;
        FirstCountTextBlock.Text = "0";
        SecondCountTextBlock.Text = "0";
        CurrentFolderTextBlock.Text = StateNotSelectedText;
        CurrentStateTextBlock.Text = StateNotSelectedText;
        CurrentStateTextBlock.Foreground = NeutralBrush;
        LoadDefaultShortcutTemplate();
        LoadLinkForCurrentMod(null);
        ClearPreview();
        UpdateProgress(0, L("当前无复制任务", "No active copy task"));

        string sourceDir = (SourceTextBox.Text ?? string.Empty).Trim();
        string targetDir = (TargetTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
        {
            StatusTextBlock.Text = L("请先选择有效的 Mod 存储文件夹。", "Choose a valid mod storage folder first.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(targetDir) && !Directory.Exists(targetDir))
        {
            StatusTextBlock.Text = L("目标文件夹不存在，请重新选择。", "The target folder does not exist. Please choose it again.");
            return;
        }

        SaveConfig();

        await Task.Run(() =>
        {
            var loadedItems = new List<FirstLevelFolderItem>();
            string[] firstDirs = Directory.GetDirectories(sourceDir);
            Array.Sort(firstDirs, StringComparer.CurrentCultureIgnoreCase);

            int secondCount = 0;
            foreach (string firstDir in firstDirs)
            {
                var item = new FirstLevelFolderItem(firstDir);
                string[] secondDirs = Directory.GetDirectories(firstDir);
                Array.Sort(secondDirs, StringComparer.CurrentCultureIgnoreCase);

                foreach (string secondDir in secondDirs)
                {
                    List<string> files = GetFiles(secondDir);
                    string state = GetFolderCopyState(targetDir, secondDir);
                    item.Children.Add(new SecondLevelFolderItem(secondDir, files, state));
                    secondCount++;
                }

                loadedItems.Add(item);
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                _allFirstLevelItems.Clear();
                _allFirstLevelItems.AddRange(loadedItems);
                ApplyFirstLevelFilter();
                FirstCountTextBlock.Text = _allFirstLevelItems.Count.ToString();
                SecondCountTextBlock.Text = secondCount.ToString();
                StatusTextBlock.Text = L(
                    $"已加载 {_allFirstLevelItems.Count} 个第一层目录，{secondCount} 个第二层目录。",
                    $"Loaded {_allFirstLevelItems.Count} first-level folders and {secondCount} second-level folders.");
                if (_firstLevelItems.Count > 0)
                {
                    FirstLevelListView.SelectedIndex = 0;
                }
            });
        });
    }

    private void PopulateSecondLevelList(FirstLevelFolderItem? firstItem, bool preserveStatus = false)
    {
        _secondLevelItems.Clear();
        ClearPreview();
        CurrentFolderTextBlock.Text = StateNotSelectedText;
        CurrentStateTextBlock.Text = StateNotSelectedText;
        CurrentStateTextBlock.Foreground = NeutralBrush;
        _currentSecondLevelPath = null;
        LoadDefaultShortcutTemplate();
        LoadLinkForCurrentMod(null);

        if (firstItem is null)
        {
            return;
        }

        foreach (SecondLevelFolderItem child in firstItem.Children)
        {
            _secondLevelItems.Add(child);
        }

        if (!preserveStatus)
        {
            StatusTextBlock.Text = L("当前第一层目录：", "Current first-level folder: ") + firstItem.Path;
        }

        if (_secondLevelItems.Count > 0)
        {
            SecondLevelListView.SelectedIndex = 0;
        }
    }

    private void ShowSecondLevelDetails(SecondLevelFolderItem? item, bool preserveStatus = false)
    {
        if (item is null)
        {
            CurrentFolderTextBlock.Text = StateNotSelectedText;
            CurrentStateTextBlock.Text = StateNotSelectedText;
            CurrentStateTextBlock.Foreground = NeutralBrush;
            ClearPreview();
            LoadLinkForCurrentMod(null);
            return;
        }

        CurrentFolderTextBlock.Text = item.Name;
        CurrentStateTextBlock.Text = item.State;
        SetStateColor(item.State);
        UpdatePreviewForDirectory(item.Path, item.Files);

        if (!preserveStatus)
        {
            StatusTextBlock.Text = L("当前查看：", "Viewing: ") + item.Path;
        }
    }

    private void SetStateColor(string? state)
    {
        if (string.Equals(state, StateCopiedText, StringComparison.CurrentCultureIgnoreCase))
        {
            CurrentStateTextBlock.Foreground = CopiedBrush;
        }
        else if (string.Equals(state, StateMissingText, StringComparison.CurrentCultureIgnoreCase))
        {
            CurrentStateTextBlock.Foreground = MissingBrush;
        }
        else
        {
            CurrentStateTextBlock.Foreground = _isDarkTheme ? new SolidColorBrush(Colors.WhiteSmoke) : NeutralBrush;
        }
    }

    private async Task ToggleDirectoryCopyAsync(SecondLevelFolderItem item)
    {
        string targetDir = (TargetTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(targetDir))
        {
            await ShowMessageAsync(
                L("请先选择有效的副文件夹。", "Choose a valid target folder first."),
                L("未设置副文件夹", "Target folder not set"));
            return;
        }

        string targetPath = GetTargetDirectoryPath(targetDir, item.Path);
        SetBusyState(true);
        try
        {
            if (Directory.Exists(targetPath))
            {
                UpdateProgress(15, L("正在移除目录...", "Removing folder..."));
                await Task.Run(() => Directory.Delete(targetPath, true));
                UpdateProgress(100, L("移除完成", "Removal complete"));
                StatusTextBlock.Text = item.Name + L(" 已从目标文件夹移除。", " was removed from the target folder.");
            }
            else
            {
                var progress = new Progress<ProgressInfo>(info => UpdateProgress(info.Percent, info.Message));
                await CopyDirectoryWithProgressAsync(item.Path, targetPath, progress);
                StatusTextBlock.Text = item.Name + L(" 已复制到目标文件夹。", " was copied to the target folder.");
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("操作失败：", "Operation failed: ") + ex.Message,
                L("错误", "Error"));
        }
        finally
        {
            SetBusyState(false);
            await RefreshListsAsync();
            SelectSecondLevelByPath(item.Path);
        }
    }

    private async Task ImportArchiveToSecondLevelFolderAsync(string archivePath, FirstLevelFolderItem item)
    {
        SetBusyState(true);
        try
        {
            string importedPath = await Task.Run(() => ImportArchiveContents(archivePath, item.Path));

            StatusTextBlock.Text = L($"已将 {Path.GetFileName(archivePath)} 解压到 {item.Name}。", $"Extracted {Path.GetFileName(archivePath)} to {item.Name}.");
            await RefreshListsAsync();
            SelectFirstLevelByPath(item.Path);
            SelectSecondLevelByPath(importedPath);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("导入压缩文件失败：", "Archive import failed: ") + ex.Message,
                L("导入失败", "Import failed"));
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private async Task ImportPreviewImageAsync(string imagePath, SecondLevelFolderItem item)
    {
        SetBusyState(true);
        try
        {
            if (!IsImageFile(imagePath))
            {
                throw new InvalidOperationException(L("拖入的文件不是受支持的图片格式。", "The dropped file is not a supported image format."));
            }

            string extension = Path.GetExtension(imagePath).ToLowerInvariant();
            await Task.Run(() =>
            {
                Directory.CreateDirectory(item.Path);
                foreach (string existingFile in Directory.GetFiles(item.Path))
                {
                    string baseName = Path.GetFileNameWithoutExtension(existingFile).ToLowerInvariant();
                    if (IsImageFile(existingFile) && (baseName == "preview" || baseName == "cover" || baseName == "thumbnail" || baseName == "image"))
                    {
                        File.Delete(existingFile);
                    }
                }

                string targetPath = Path.Combine(item.Path, "preview" + extension);
                File.Copy(imagePath, targetPath, true);
            });

            StatusTextBlock.Text = item.Name + L(" 的预览图已更新。", "'s preview image was updated.");
            await RefreshListsAsync();
            SelectSecondLevelByPath(item.Path);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("导入预览图失败：", "Preview import failed: ") + ex.Message,
                L("导入失败", "Import failed"));
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private async Task CopyDirectoryWithProgressAsync(string sourcePath, string targetPath, IProgress<ProgressInfo> progress)
    {
        await Task.Run(() =>
        {
            string[] directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
            string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            int totalSteps = directories.Length + files.Length + 1;
            int currentStep = 0;

            Directory.CreateDirectory(targetPath);
            currentStep++;
            progress.Report(new ProgressInfo(currentStep * 100 / totalSteps, L("正在创建目录...", "Creating folder...")));

            foreach (string directory in directories)
            {
                string relativePath = directory[sourcePath.Length..].TrimStart(Path.DirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(targetPath, relativePath));
                currentStep++;
                progress.Report(new ProgressInfo(currentStep * 100 / totalSteps, L("正在创建子目录...", "Creating subfolders...")));
            }

            foreach (string file in files)
            {
                string relativePath = file[sourcePath.Length..].TrimStart(Path.DirectorySeparatorChar);
                string destinationFile = Path.Combine(targetPath, relativePath);
                string? destinationDir = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrEmpty(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                File.Copy(file, destinationFile, true);
                currentStep++;
                progress.Report(new ProgressInfo(currentStep * 100 / totalSteps, L("正在复制文件...", "Copying files...")));
            }

            progress.Report(new ProgressInfo(100, L("复制完成", "Copy complete")));
        });
    }

    private void UpdateProgress(int percent, string message)
    {
        CopyProgressBar.Value = percent;
        ProgressTextBlock.Text = message;
    }

    private void SetBusyState(bool busy)
    {
        ThemeToggleButton.IsEnabled = !busy;
        LanguageToggleButton.IsEnabled = !busy;
        FirstLevelListView.IsEnabled = !busy;
        SecondLevelListView.IsEnabled = !busy;
        OpenModLinkButton.IsEnabled = !busy && !string.IsNullOrWhiteSpace(ModLinkTextBox.Text);
    }

    private void LoadDefaultShortcutTemplate()
    {
        _isLoadingBindings = true;
        _activeShortcutKeyBox = null;
        foreach (TextBox box in _shortcutKeyBoxes)
        {
            box.Text = string.Empty;
        }

        foreach (TextBox box in _shortcutActionBoxes)
        {
            box.Text = string.Empty;
        }

        _visibleShortcutRows = 1;
        UpdateShortcutRowVisibility();
        UpdateShortcutKeyFocusVisuals();
        _isLoadingBindings = false;
    }

    private void LoadBindingsForCurrentMod(SecondLevelFolderItem? item)
    {
        _isLoadingBindings = true;
        _activeShortcutKeyBox = null;

        foreach (TextBox box in _shortcutKeyBoxes)
        {
            box.Text = string.Empty;
        }

        foreach (TextBox box in _shortcutActionBoxes)
        {
            box.Text = string.Empty;
        }

        if (item is not null && _modBindings.TryGetValue(item.Path, out List<ShortcutBinding>? bindings) && bindings.Count > 0)
        {
            _visibleShortcutRows = Math.Min(MaxShortcutRows, Math.Max(1, bindings.Count));
            for (int i = 0; i < _visibleShortcutRows; i++)
            {
                _shortcutKeyBoxes[i].Text = bindings[i].Shortcut;
                _shortcutActionBoxes[i].Text = bindings[i].Action;
            }
        }
        else
        {
            _visibleShortcutRows = 1;
        }

        UpdateShortcutRowVisibility();
        UpdateShortcutKeyFocusVisuals();
        _isLoadingBindings = false;
    }

    private void SaveCurrentModBindings()
    {
        if (_isLoadingBindings || string.IsNullOrEmpty(_currentSecondLevelPath))
        {
            return;
        }

        var bindings = new List<ShortcutBinding>();
        for (int i = 0; i < _visibleShortcutRows; i++)
        {
            string shortcut = (_shortcutKeyBoxes[i].Text ?? string.Empty).Trim();
            string action = (_shortcutActionBoxes[i].Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(shortcut) || !string.IsNullOrWhiteSpace(action))
            {
                bindings.Add(new ShortcutBinding(shortcut, action));
            }
        }

        if (bindings.Count == 0)
        {
            _modBindings.Remove(_currentSecondLevelPath);
        }
        else
        {
            _modBindings[_currentSecondLevelPath] = bindings;
        }

        SaveConfig();
        UpdateShortcutRowVisibility();
    }

    private void LoadLinkForCurrentMod(SecondLevelFolderItem? item)
    {
        _isLoadingModLink = true;
        ModLinkTextBox.Text = item is not null && _modLinks.TryGetValue(item.Path, out string? link) ? link : string.Empty;
        _isLoadingModLink = false;
        UpdateLinkButtonState();
    }

    private void SaveCurrentModLink()
    {
        if (_isLoadingModLink || string.IsNullOrEmpty(_currentSecondLevelPath))
        {
            UpdateLinkButtonState();
            return;
        }

        string link = (ModLinkTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(link))
        {
            _modLinks.Remove(_currentSecondLevelPath);
        }
        else
        {
            _modLinks[_currentSecondLevelPath] = link;
        }

        SaveConfig();
        UpdateLinkButtonState();
    }

    private void UpdateLinkButtonState()
    {
        OpenModLinkButton.IsEnabled = !string.IsNullOrWhiteSpace(ModLinkTextBox.Text);
    }

    private void UpdateShortcutRowVisibility()
    {
        for (int i = 0; i < MaxShortcutRows; i++)
        {
            ShortcutRowsPanel.Children[i].Visibility = i < _visibleShortcutRows ? Visibility.Visible : Visibility.Collapsed;
        }

        ShortcutRowsTextBlock.Text = L($"当前 {_visibleShortcutRows} / {MaxShortcutRows} 行", $"{_visibleShortcutRows} / {MaxShortcutRows} rows");
    }

    private async Task<bool> TryRunBoundShortcutAsync(string shortcut)
    {
        string normalized = NormalizeShortcut(shortcut);
        foreach ((string path, List<ShortcutBinding> bindings) in _modBindings)
        {
            if (bindings.Any(binding => NormalizeShortcut(binding.Shortcut) == normalized))
            {
                SecondLevelFolderItem? secondItem = FindSecondLevelByPath(path);
                if (secondItem is null)
                {
                    continue;
                }

                SelectSecondLevelByPath(secondItem.Path);
                await EnsureDirectoryCopiedAsync(secondItem);
                return true;
            }
        }

        return false;
    }

    private async Task EnsureDirectoryCopiedAsync(SecondLevelFolderItem item)
    {
        string targetDir = (TargetTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(targetDir))
        {
            StatusTextBlock.Text = L("快捷键执行失败：请先设置有效的目标文件夹。", "Shortcut failed: choose a valid target folder first.");
            return;
        }

        string targetPath = GetTargetDirectoryPath(targetDir, item.Path);
        if (Directory.Exists(targetPath))
        {
            StatusTextBlock.Text = item.Name + L(" 已通过快捷键定位，当前已经复制。", " was located by shortcut and is already copied.");
            return;
        }

        await ToggleDirectoryCopyAsync(item);
    }

    private void SelectSecondLevelByPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        for (int firstIndex = 0; firstIndex < _firstLevelItems.Count; firstIndex++)
        {
            FirstLevelFolderItem first = _firstLevelItems[firstIndex];
            if (first.Children.Any(child => string.Equals(child.Path, path, StringComparison.CurrentCultureIgnoreCase)))
            {
                FirstLevelListView.SelectedIndex = firstIndex;
                for (int secondIndex = 0; secondIndex < _secondLevelItems.Count; secondIndex++)
                {
                    if (string.Equals(_secondLevelItems[secondIndex].Path, path, StringComparison.CurrentCultureIgnoreCase))
                    {
                        SecondLevelListView.SelectedIndex = secondIndex;
                        SecondLevelListView.ScrollIntoView(_secondLevelItems[secondIndex]);
                        return;
                    }
                }
            }
        }
    }

    private static string BuildShortcutFromEvent(KeyRoutedEventArgs e)
    {
        var parts = new List<string>();

        if (IsModifierDown(VirtualKey.Control))
        {
            parts.Add("Ctrl");
        }

        if (IsModifierDown(VirtualKey.Shift))
        {
            parts.Add("Shift");
        }

        if (IsModifierDown(VirtualKey.Menu))
        {
            parts.Add("Alt");
        }

        string key = KeyToString(e.Key);
        if (string.IsNullOrEmpty(key) || key is "Control" or "Shift" or "Alt" or "LeftWindows" or "RightWindows")
        {
            return string.Empty;
        }

        parts.Add(key);
        return string.Join("+", parts);
    }

    private static bool IsModifierDown(VirtualKey key)
    {
        CoreVirtualKeyStates state = InputKeyboardSource.GetKeyStateForCurrentThread(key);
        return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }

    private static string KeyToString(VirtualKey key)
    {
        if (key is >= VirtualKey.A and <= VirtualKey.Z)
        {
            return key.ToString().ToUpperInvariant();
        }

        if (key is >= VirtualKey.Number0 and <= VirtualKey.Number9)
        {
            return ((int)key - (int)VirtualKey.Number0).ToString();
        }

        if (key is >= VirtualKey.F1 and <= VirtualKey.F24)
        {
            return key.ToString();
        }

        if (key is >= VirtualKey.NumberPad0 and <= VirtualKey.NumberPad9)
        {
            return "Num" + ((int)key - (int)VirtualKey.NumberPad0);
        }

        int keyCode = (int)key;
        if (keyCode is >= 186 and <= 192 or >= 219 and <= 222)
        {
            return keyCode switch
            {
                186 => ";",
                187 => "=",
                188 => ",",
                189 => "-",
                190 => ".",
                191 => "/",
                192 => "`",
                219 => "[",
                220 => "\\",
                221 => "]",
                222 => "'",
                _ => string.Empty
            };
        }

        return key switch
        {
            VirtualKey.Enter => "Enter",
            VirtualKey.Escape => "Esc",
            VirtualKey.Space => "Space",
            VirtualKey.Left => "Left",
            VirtualKey.Right => "Right",
            VirtualKey.Up => "Up",
            VirtualKey.Down => "Down",
            VirtualKey.Tab => "Tab",
            VirtualKey.Home => "Home",
            VirtualKey.End => "End",
            VirtualKey.PageUp => "PageUp",
            VirtualKey.PageDown => "PageDown",
            VirtualKey.Insert => "Insert",
            VirtualKey.Delete => "Delete",
            VirtualKey.Decimal => ".",
            VirtualKey.Add => "+",
            VirtualKey.Subtract => "-",
            VirtualKey.Multiply => "*",
            VirtualKey.Divide => "/",
            _ => key.ToString()
        };
    }

    private static string NormalizeShortcut(string? shortcut)
    {
        return string.Join("+", (shortcut ?? string.Empty)
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part =>
            {
                if (string.Equals(part, "control", StringComparison.OrdinalIgnoreCase))
                {
                    return "Ctrl";
                }

                if (string.Equals(part, "menu", StringComparison.OrdinalIgnoreCase))
                {
                    return "Alt";
                }

                return part.Length == 1 ? part.ToUpperInvariant() : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant();
            }));
    }

    private static List<string> GetFiles(string directory)
    {
        string[] files = Directory.GetFiles(directory);
        Array.Sort(files, StringComparer.CurrentCultureIgnoreCase);
        return files.ToList();
    }

    private static string GetTargetDirectoryPath(string targetDir, string sourceDirectoryPath)
    {
        return Path.Combine(targetDir, Path.GetFileName(sourceDirectoryPath));
    }

    private string GetFolderCopyState(string targetDir, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(targetDir))
        {
            return StateNotConfiguredText;
        }

        return Directory.Exists(GetTargetDirectoryPath(targetDir, directoryPath)) ? StateCopiedText : StateMissingText;
    }

    private void RefreshLocalizedStates()
    {
        string targetDir = (TargetTextBox.Text ?? string.Empty).Trim();
        foreach (FirstLevelFolderItem first in _firstLevelItems)
        {
            foreach (SecondLevelFolderItem second in first.Children)
            {
                second.State = GetFolderCopyState(targetDir, second.Path);
            }
        }
    }

    private SecondLevelFolderItem? FindSecondLevelByPath(string path)
    {
        foreach (FirstLevelFolderItem first in _firstLevelItems)
        {
            foreach (SecondLevelFolderItem second in first.Children)
            {
                if (string.Equals(second.Path, path, StringComparison.CurrentCultureIgnoreCase))
                {
                    return second;
                }
            }
        }

        return null;
    }

    private SecondLevelFolderItem? GetSelectedSecondLevelItem()
    {
        return SecondLevelListView.SelectedItem as SecondLevelFolderItem;
    }

    private void UpdatePreviewForDirectory(string directoryPath, List<string> files)
    {
        SetPreviewImage(FindPreviewImage(directoryPath, files));
    }

    private void SetPreviewImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            PreviewImage.Source = null;
            PreviewHintTextBlock.Text = L("当前第二层文件夹未找到可预览图片", "No preview image was found for the current second-level folder.");
            PreviewHintTextBlock.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            PreviewImage.Source = new BitmapImage(new Uri(imagePath));
            PreviewHintTextBlock.Visibility = Visibility.Collapsed;
        }
        catch
        {
            PreviewImage.Source = null;
            PreviewHintTextBlock.Text = L("图片无法读取或格式不受支持", "The image could not be loaded or is not supported.");
            PreviewHintTextBlock.Visibility = Visibility.Visible;
        }
    }

    private void ClearPreview()
    {
        PreviewImage.Source = null;
        PreviewHintTextBlock.Text = L("当前第二层文件夹未找到可预览图片", "No preview image was found for the current second-level folder.");
        PreviewHintTextBlock.Visibility = Visibility.Visible;
    }

    private static string? FindPreviewImage(string directoryPath, List<string> files)
    {
        string? namedPreview = files.FirstOrDefault(file =>
        {
            string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            return IsImageFile(file) && (name == "preview" || name == "cover" || name == "thumbnail" || name == "image");
        });

        if (!string.IsNullOrEmpty(namedPreview))
        {
            return namedPreview;
        }

        string[] images = Directory.GetFiles(directoryPath)
            .Where(IsImageFile)
            .OrderBy(path => path, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        return images.FirstOrDefault();
    }

    private static bool IsImageFile(string path)
    {
        string extension = Path.GetExtension(path);
        return !string.IsNullOrEmpty(extension) && ImageExtensions.Contains(extension.ToLowerInvariant());
    }

    private static bool IsSupportedArchiveFile(string path)
    {
        string fileName = Path.GetFileName(path);
        return SupportedArchiveExtensions.Any(extension =>
            fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeLink(string link)
    {
        string trimmed = link.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return "https://" + trimmed;
    }

    private void ApplyFirstLevelFilter()
    {
        string keyword = (FirstLevelSearchTextBox.Text ?? string.Empty).Trim();
        string? selectedPath = FirstLevelListView.SelectedItem is FirstLevelFolderItem selected ? selected.Path : null;

        _firstLevelItems.Clear();

        IEnumerable<FirstLevelFolderItem> filtered = string.IsNullOrWhiteSpace(keyword)
            ? _allFirstLevelItems
            : _allFirstLevelItems.Where(item =>
                item.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));

        foreach (FirstLevelFolderItem item in filtered)
        {
            _firstLevelItems.Add(item);
        }

        if (!string.IsNullOrEmpty(selectedPath))
        {
            SelectFirstLevelByPath(selectedPath);
        }

        if (FirstLevelListView.SelectedItem is null && _firstLevelItems.Count > 0)
        {
            FirstLevelListView.SelectedIndex = 0;
        }
    }

    private void SelectFirstLevelByPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        FirstLevelFolderItem? item = _firstLevelItems.FirstOrDefault(first =>
            string.Equals(first.Path, path, StringComparison.CurrentCultureIgnoreCase));
        if (item is not null)
        {
            FirstLevelListView.SelectedItem = item;
        }
    }

    private async Task ImportArchiveToSelectedFirstLevelFolderAsync(string archivePath, FirstLevelFolderItem item)
    {
        SetBusyState(true);
        try
        {
            await Task.Run(() => ExtractArchiveToDirectory(archivePath, item.Path));

            StatusTextBlock.Text = L($"已将 {Path.GetFileName(archivePath)} 导入到 {item.Name}。", $"Imported {Path.GetFileName(archivePath)} into {item.Name}.");
            await RefreshListsAsync();
            SelectFirstLevelByPath(item.Path);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync(
                L("导入压缩文件失败：", "Archive import failed: ") + ex.Message,
                L("导入失败", "Import failed"));
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private string ImportArchiveContents(string archivePath, string firstLevelDirectory)
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), "ModFolderCopier_Import_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            ExtractArchiveToDirectory(archivePath, tempDirectory);

            string[] rootDirectories = Directory.GetDirectories(tempDirectory);
            string[] rootFiles = Directory.GetFiles(tempDirectory);

            if (rootFiles.Length == 0 && rootDirectories.Length == 1)
            {
                string sourceDirectory = rootDirectories[0];
                string destinationPath = Path.Combine(firstLevelDirectory, Path.GetFileName(sourceDirectory));
                EnsureImportDestinationDoesNotExist(destinationPath);
                Directory.Move(sourceDirectory, destinationPath);
                return destinationPath;
            }

            string importedDirectory = Path.Combine(firstLevelDirectory, BuildImportedFolderName(archivePath));
            EnsureImportDestinationDoesNotExist(importedDirectory);
            Directory.CreateDirectory(importedDirectory);

            foreach (string directory in rootDirectories)
            {
                Directory.Move(directory, Path.Combine(importedDirectory, Path.GetFileName(directory)));
            }

            foreach (string file in rootFiles)
            {
                File.Move(file, Path.Combine(importedDirectory, Path.GetFileName(file)));
            }

            return importedDirectory;
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    private static void EnsureImportDestinationDoesNotExist(string destinationPath)
    {
        if (Directory.Exists(destinationPath) || File.Exists(destinationPath))
        {
            throw new InvalidOperationException($"\"{Path.GetFileName(destinationPath)}\" already exists.");
        }
    }

    private static string BuildImportedFolderName(string archivePath)
    {
        string fileName = Path.GetFileName(archivePath);

        foreach (string extension in SupportedArchiveExtensions.OrderByDescending(item => item.Length))
        {
            if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName[..^extension.Length];
                break;
            }
        }

        if (fileName.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName[..^4];
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        fileName = fileName.Trim();
        return string.IsNullOrWhiteSpace(fileName) ? "ImportedMod" : fileName;
    }

    private void ExtractArchiveToDirectory(string archivePath, string destinationDirectory)
    {
        string extension = Path.GetExtension(archivePath).ToLowerInvariant();
        DispatcherQueue.TryEnqueue(() => UpdateProgress(10, L("正在准备解压...", "Preparing extraction...")));

        if (extension == ".zip")
        {
            using ZipArchive archive = ZipFile.OpenRead(archivePath);
            List<ZipArchiveEntry> entries = archive.Entries
                .Where(entry => !string.IsNullOrEmpty(entry.FullName) && !entry.FullName.EndsWith("/", StringComparison.Ordinal))
                .ToList();

            if (entries.Count == 0)
            {
                throw new InvalidOperationException(L("这个压缩包里没有可解压的文件。", "This archive does not contain extractable files."));
            }

            for (int index = 0; index < entries.Count; index++)
            {
                ZipArchiveEntry entry = entries[index];
                string destinationPath = Path.Combine(destinationDirectory, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                string? destinationFolder = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                entry.ExtractToFile(destinationPath, true);
                int percent = Math.Min(95, 10 + ((index + 1) * 85 / entries.Count));
                DispatcherQueue.TryEnqueue(() => UpdateProgress(percent, L("正在解压文件...", "Extracting files...")));
            }

            DispatcherQueue.TryEnqueue(() => UpdateProgress(100, L("解压完成", "Extraction complete")));
            return;
        }

        RunTarExtraction(archivePath, destinationDirectory);
        DispatcherQueue.TryEnqueue(() => UpdateProgress(100, L("解压完成", "Extraction complete")));
    }

    private void RunTarExtraction(string archivePath, string destinationDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "tar.exe",
            Arguments = $"-xf \"{archivePath}\" -C \"{destinationDirectory}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(L("无法启动系统解压工具 tar.exe。", "Unable to start tar.exe."));
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? L("当前系统无法解压该格式，建议尝试 ZIP 或 7Z。", "This system could not extract the selected format. Try ZIP or 7Z instead.")
                : error.Trim());
        }
    }

    private async Task ShowMessageAsync(string content, string title)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = L("确定", "OK"),
            XamlRoot = RootGrid.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async Task<string?> PromptForTextAsync(string content, string title, string defaultValue)
    {
        var textBox = new TextBox
        {
            Text = defaultValue,
            PlaceholderText = L("输入名称", "Enter a name"),
            Margin = new Thickness(0, 12, 0, 0)
        };

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text = content,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(textBox);

        var dialog = new ContentDialog
        {
            Title = title,
            Content = panel,
            PrimaryButtonText = L("确定", "OK"),
            CloseButtonText = L("取消", "Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? textBox.Text : null;
    }

    private async Task<bool> ShowConfirmAsync(string content, string title)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new TextBlock
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap
            },
            PrimaryButtonText = L("确认", "Confirm"),
            CloseButtonText = L("取消", "Cancel"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}

public sealed class FirstLevelFolderItem
{
    public FirstLevelFolderItem(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(path);
    }

    public string Name { get; }

    public string Path { get; }

    public List<SecondLevelFolderItem> Children { get; } = [];

    public int ChildrenCount => Children.Count;
}

public sealed class SecondLevelFolderItem
{
    public SecondLevelFolderItem(string path, List<string> files, string state)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(path);
        Files = files;
        State = state;
    }

    public string Name { get; }

    public string Path { get; }

    public List<string> Files { get; }

    public string State { get; set; }

    public int FilesCount => Files.Count;
}

public sealed class ShortcutBinding
{
    public ShortcutBinding(string shortcut, string action)
    {
        Shortcut = shortcut;
        Action = action;
    }

    public string Shortcut { get; }

    public string Action { get; }
}

public sealed class ProgressInfo
{
    public ProgressInfo(int percent, string message)
    {
        Percent = percent;
        Message = message;
    }

    public int Percent { get; }

    public string Message { get; }
}
