using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
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
    private const string AppVersion = "v2.2";
    private const int MaxShortcutRows = 10;

    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"];
    private static readonly SolidColorBrush CopiedBrush = new(ColorHelper.FromArgb(255, 36, 124, 76));
    private static readonly SolidColorBrush MissingBrush = new(ColorHelper.FromArgb(255, 177, 76, 55));
    private static readonly SolidColorBrush NeutralBrush = new(ColorHelper.FromArgb(255, 60, 60, 60));

    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");
    private readonly ObservableCollection<FirstLevelFolderItem> _firstLevelItems = [];
    private readonly ObservableCollection<SecondLevelFolderItem> _secondLevelItems = [];
    private readonly List<TextBox> _shortcutKeyBoxes = [];
    private readonly List<TextBox> _shortcutActionBoxes = [];
    private readonly Dictionary<string, List<ShortcutBinding>> _modBindings = new(StringComparer.CurrentCultureIgnoreCase);

    private bool _isDarkTheme;
    private bool _isLoadingBindings;
    private int _visibleShortcutRows = 1;
    private string? _currentSecondLevelPath;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Mod 文件复制器 " + AppVersion;
        ExtendsContentIntoTitleBar = false;
        FirstLevelListView.ItemsSource = _firstLevelItems;
        SecondLevelListView.ItemsSource = _secondLevelItems;
        BuildShortcutRows();
        ApplyTheme(false);
        Activated += (_, _) => RootGrid.Focus(FocusState.Programmatic);
        LoadConfig();
        if (Directory.Exists(SourceTextBox.Text))
        {
            _ = RefreshListsAsync();
        }
    }

    private void BuildShortcutRows()
    {
        ShortcutRowsPanel.Children.Clear();
        _shortcutKeyBoxes.Clear();
        _shortcutActionBoxes.Clear();

        for (int index = 0; index < MaxShortcutRows; index++)
        {
            var rowGrid = new Grid { ColumnSpacing = 12 };
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var keyBorder = CreateInsetBorder();
            var keyBox = new TextBox
            {
                PlaceholderText = "快捷键",
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 8, 10, 8)
            };
            keyBox.TextChanged += OnShortcutTextChanged;
            keyBorder.Child = keyBox;

            var actionBorder = CreateInsetBorder();
            var actionBox = new TextBox
            {
                PlaceholderText = "功能",
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 8, 10, 8)
            };
            actionBox.TextChanged += OnShortcutTextChanged;
            actionBorder.Child = actionBox;

            Grid.SetColumn(keyBorder, 0);
            Grid.SetColumn(actionBorder, 1);
            rowGrid.Children.Add(keyBorder);
            rowGrid.Children.Add(actionBorder);

            ShortcutRowsPanel.Children.Add(rowGrid);
            _shortcutKeyBoxes.Add(keyBox);
            _shortcutActionBoxes.Add(actionBox);
        }

        UpdateShortcutRowVisibility();
    }

    private static Border CreateInsetBorder()
    {
        return new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            Background = (Brush)Application.Current.Resources["AppInsetBackgroundBrush"],
            BorderBrush = (Brush)Application.Current.Resources["AppCardBorderBrush"]
        };
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

    private void OnOpenSourceClicked(object sender, RoutedEventArgs e) => OpenDirectory(SourceTextBox.Text, "主文件夹");

    private void OnOpenTargetClicked(object sender, RoutedEventArgs e) => OpenDirectory(TargetTextBox.Text, "副文件夹");

    private void OnOpenLauncherClicked(object sender, RoutedEventArgs e) => OpenFileLocation(LauncherTextBox.Text, "启动器");

    private async void OnRunLauncherClicked(object sender, RoutedEventArgs e)
    {
        string launcherPath = (LauncherTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(launcherPath) || !File.Exists(launcherPath))
        {
            await ShowMessageAsync("请先设置有效的 XXMI 启动器路径。", "未设置启动器");
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
            StatusTextBlock.Text = "已启动 XXMI 启动器。";
            SaveConfig();
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("运行启动器失败：" + ex.Message, "启动失败");
        }
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e) => await RefreshListsAsync();

    private async void OnImportZipClicked(object sender, RoutedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        if (item is null)
        {
            await ShowMessageAsync("请先选择一个第二层 Mod。", "未选择 Mod");
            return;
        }

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".zip");
        picker.SuggestedStartLocation = PickerLocationId.Downloads;
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        StorageFile? file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            await ImportZipToFolderAsync(file.Path, item);
        }
    }

    private async void OnToggleCopyClicked(object sender, RoutedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        if (item is null)
        {
            await ShowMessageAsync("请先选择一个第二层 Mod。", "未选择 Mod");
            return;
        }

        await ToggleDirectoryCopyAsync(item);
    }

    private void OnFirstLevelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PopulateSecondLevelList(FirstLevelListView.SelectedItem as FirstLevelFolderItem);
    }

    private void OnSecondLevelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        _currentSecondLevelPath = item?.Path;
        ShowSecondLevelDetails(item);
        LoadBindingsForCurrentMod(item);
    }

    private async void OnSecondLevelDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        var item = GetSelectedSecondLevelItem();
        if (item is not null)
        {
            await ToggleDirectoryCopyAsync(item);
        }
    }

    private async void OnAddShortcutRowClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentSecondLevelPath))
        {
            await ShowMessageAsync("请先选择一个第二层 Mod。", "未选择 Mod");
            return;
        }

        if (_visibleShortcutRows >= MaxShortcutRows)
        {
            StatusTextBlock.Text = "快捷键行数已达到上限 10 行。";
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

    private void OnThemeToggleClicked(object sender, RoutedEventArgs e)
    {
        ApplyTheme(!_isDarkTheme);
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
            await ShowMessageAsync("请先选择一个第二层 Mod，再把图片拖到预览区。", "未选择 Mod");
            return;
        }

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            await ShowMessageAsync("只支持从资源管理器拖入图片文件。", "不支持的拖放内容");
            return;
        }

        IReadOnlyList<IStorageItem> droppedItems = await e.DataView.GetStorageItemsAsync();
        StorageFile? imageFile = droppedItems
            .OfType<StorageFile>()
            .FirstOrDefault(file => IsImageFile(file.Path));

        if (imageFile is null)
        {
            await ShowMessageAsync("没有检测到可用的图片文件。", "未找到图片");
            return;
        }

        await ImportPreviewImageAsync(imageFile.Path, item);
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

    private void ApplyTheme(bool dark)
    {
        _isDarkTheme = dark;
        RootGrid.RequestedTheme = dark ? ElementTheme.Dark : ElementTheme.Light;
        ThemeToggleButton.Content = dark ? "切换浅色" : "切换深色";
        SetStateColor(CurrentStateTextBlock.Text);
    }

    private async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.Desktop;
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        StorageFolder? folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    private async Task<string?> PickFileAsync(IReadOnlyList<string> fileTypes)
    {
        var picker = new FileOpenPicker();
        foreach (string fileType in fileTypes)
        {
            picker.FileTypeFilter.Add(fileType);
        }

        picker.SuggestedStartLocation = PickerLocationId.Desktop;
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
                _ = ShowMessageAsync(title + "不存在，请重新选择。", "路径无效");
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
            _ = ShowMessageAsync("打开目录失败：" + ex.Message, "操作失败");
        }
    }

    private void OpenFileLocation(string? filePath, string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                _ = ShowMessageAsync(title + "不存在，请重新选择。", "路径无效");
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
            _ = ShowMessageAsync("打开启动器位置失败：" + ex.Message, "操作失败");
        }
    }

    private void LoadConfig()
    {
        _modBindings.Clear();

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
                    ApplyTheme(string.Equals(DecodeValue(line["theme=".Length..]), "dark", StringComparison.OrdinalIgnoreCase));
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
            }
        }
        catch
        {
            StatusTextBlock.Text = "配置读取失败，已忽略旧配置。";
        }

        LoadDefaultShortcutTemplate();
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
                "theme=" + EncodeValue(_isDarkTheme ? "dark" : "light")
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

            File.WriteAllLines(_configPath, lines);
        }
        catch
        {
            StatusTextBlock.Text = "配置保存失败，但不影响当前使用。";
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
        CurrentFolderTextBlock.Text = "未选择";
        CurrentStateTextBlock.Text = "未选择";
        CurrentStateTextBlock.Foreground = NeutralBrush;
        LoadDefaultShortcutTemplate();
        ClearPreview();
        UpdateProgress(0, "当前无复制任务");

        string sourceDir = (SourceTextBox.Text ?? string.Empty).Trim();
        string targetDir = (TargetTextBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
        {
            StatusTextBlock.Text = "请先选择有效的主文件夹。";
            return;
        }

        if (!string.IsNullOrWhiteSpace(targetDir) && !Directory.Exists(targetDir))
        {
            StatusTextBlock.Text = "副文件夹不存在，请重新选择。";
            return;
        }

        SaveConfig();

        await Task.Run(() =>
        {
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

                DispatcherQueue.TryEnqueue(() => _firstLevelItems.Add(item));
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                FirstCountTextBlock.Text = _firstLevelItems.Count.ToString();
                SecondCountTextBlock.Text = secondCount.ToString();
                StatusTextBlock.Text = $"已加载 {_firstLevelItems.Count} 个第一层目录，{secondCount} 个第二层目录。";
                if (_firstLevelItems.Count > 0)
                {
                    FirstLevelListView.SelectedIndex = 0;
                }
            });
        });
    }

    private void PopulateSecondLevelList(FirstLevelFolderItem? firstItem)
    {
        _secondLevelItems.Clear();
        ClearPreview();
        CurrentFolderTextBlock.Text = "未选择";
        CurrentStateTextBlock.Text = "未选择";
        CurrentStateTextBlock.Foreground = NeutralBrush;
        _currentSecondLevelPath = null;
        LoadDefaultShortcutTemplate();

        if (firstItem is null)
        {
            return;
        }

        foreach (SecondLevelFolderItem child in firstItem.Children)
        {
            _secondLevelItems.Add(child);
        }

        StatusTextBlock.Text = "当前第一层目录：" + firstItem.Path;
        if (_secondLevelItems.Count > 0)
        {
            SecondLevelListView.SelectedIndex = 0;
        }
    }

    private void ShowSecondLevelDetails(SecondLevelFolderItem? item)
    {
        if (item is null)
        {
            CurrentFolderTextBlock.Text = "未选择";
            CurrentStateTextBlock.Text = "未选择";
            CurrentStateTextBlock.Foreground = NeutralBrush;
            ClearPreview();
            return;
        }

        CurrentFolderTextBlock.Text = item.Name;
        CurrentStateTextBlock.Text = item.State;
        SetStateColor(item.State);
        UpdatePreviewForDirectory(item.Path, item.Files);
        StatusTextBlock.Text = "当前查看：" + item.Path;
    }

    private void SetStateColor(string? state)
    {
        if (string.Equals(state, "已复制", StringComparison.CurrentCultureIgnoreCase))
        {
            CurrentStateTextBlock.Foreground = CopiedBrush;
        }
        else if (string.Equals(state, "未复制", StringComparison.CurrentCultureIgnoreCase))
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
            await ShowMessageAsync("请先选择有效的副文件夹。", "未设置副文件夹");
            return;
        }

        string targetPath = GetTargetDirectoryPath(targetDir, item.Path);
        SetBusyState(true);
        try
        {
            if (Directory.Exists(targetPath))
            {
                UpdateProgress(15, "正在移除目录...");
                await Task.Run(() => Directory.Delete(targetPath, true));
                UpdateProgress(100, "移除完成");
                StatusTextBlock.Text = item.Name + " 已从副文件夹移除。";
            }
            else
            {
                var progress = new Progress<ProgressInfo>(info => UpdateProgress(info.Percent, info.Message));
                await CopyDirectoryWithProgressAsync(item.Path, targetPath, progress);
                StatusTextBlock.Text = item.Name + " 已复制到副文件夹。";
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("操作失败：" + ex.Message, "错误");
        }
        finally
        {
            SetBusyState(false);
            await RefreshListsAsync();
            SelectSecondLevelByPath(item.Path);
        }
    }

    private async Task ImportZipToFolderAsync(string zipPath, SecondLevelFolderItem item)
    {
        SetBusyState(true);
        try
        {
            await Task.Run(() =>
            {
                using ZipArchive archive = ZipFile.OpenRead(zipPath);
                List<ZipArchiveEntry> entries = archive.Entries
                    .Where(entry => !string.IsNullOrEmpty(entry.FullName) && !entry.FullName.EndsWith("/", StringComparison.Ordinal))
                    .ToList();

                if (entries.Count == 0)
                {
                    throw new InvalidOperationException("这个 ZIP 里没有可解压的文件。");
                }

                for (int index = 0; index < entries.Count; index++)
                {
                    ZipArchiveEntry entry = entries[index];
                    string destinationPath = Path.Combine(item.Path, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                    string? destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    entry.ExtractToFile(destinationPath, true);
                    int percent = Math.Min(100, (index + 1) * 100 / entries.Count);
                    DispatcherQueue.TryEnqueue(() => UpdateProgress(percent, "正在解压文件..."));
                }
            });

            StatusTextBlock.Text = $"已将 {Path.GetFileName(zipPath)} 解压到 {item.Name}。";
            await RefreshListsAsync();
            SelectSecondLevelByPath(item.Path);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("导入 ZIP 失败：" + ex.Message, "导入失败");
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
                throw new InvalidOperationException("拖入的文件不是受支持的图片格式。");
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

            StatusTextBlock.Text = item.Name + " 的预览图已更新。";
            await RefreshListsAsync();
            SelectSecondLevelByPath(item.Path);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("导入预览图失败：" + ex.Message, "导入失败");
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
            progress.Report(new ProgressInfo(currentStep * 100 / totalSteps, "正在创建目录..."));

            foreach (string directory in directories)
            {
                string relativePath = directory[sourcePath.Length..].TrimStart(Path.DirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(targetPath, relativePath));
                currentStep++;
                progress.Report(new ProgressInfo(currentStep * 100 / totalSteps, "正在创建子目录..."));
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
                progress.Report(new ProgressInfo(currentStep * 100 / totalSteps, "正在复制文件..."));
            }

            progress.Report(new ProgressInfo(100, "复制完成"));
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
        FirstLevelListView.IsEnabled = !busy;
        SecondLevelListView.IsEnabled = !busy;
    }

    private void LoadDefaultShortcutTemplate()
    {
        _isLoadingBindings = true;
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
        _isLoadingBindings = false;
    }

    private void LoadBindingsForCurrentMod(SecondLevelFolderItem? item)
    {
        _isLoadingBindings = true;

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

    private void UpdateShortcutRowVisibility()
    {
        for (int i = 0; i < MaxShortcutRows; i++)
        {
            ShortcutRowsPanel.Children[i].Visibility = i < _visibleShortcutRows ? Visibility.Visible : Visibility.Collapsed;
        }

        ShortcutRowsTextBlock.Text = $"当前 {_visibleShortcutRows} / {MaxShortcutRows} 行";
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
            StatusTextBlock.Text = "快捷键执行失败：请先设置有效的副文件夹。";
            return;
        }

        string targetPath = GetTargetDirectoryPath(targetDir, item.Path);
        if (Directory.Exists(targetPath))
        {
            StatusTextBlock.Text = item.Name + " 已通过快捷键定位，当前已经复制。";
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
        if (string.IsNullOrEmpty(key) || key is "Control" or "Shift" or "Alt")
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

        return key switch
        {
            VirtualKey.Enter => "Enter",
            VirtualKey.Escape => "Esc",
            VirtualKey.Space => "Space",
            VirtualKey.Left => "Left",
            VirtualKey.Right => "Right",
            VirtualKey.Up => "Up",
            VirtualKey.Down => "Down",
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

    private static string GetFolderCopyState(string targetDir, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(targetDir))
        {
            return "未设置";
        }

        return Directory.Exists(GetTargetDirectoryPath(targetDir, directoryPath)) ? "已复制" : "未复制";
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
            PreviewHintTextBlock.Text = "图片无法读取或格式不受支持";
            PreviewHintTextBlock.Visibility = Visibility.Visible;
        }
    }

    private void ClearPreview()
    {
        PreviewImage.Source = null;
        PreviewHintTextBlock.Text = "当前第二层文件夹未找到可预览图片";
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

    private async Task ShowMessageAsync(string content, string title)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "确定",
            XamlRoot = RootGrid.XamlRoot
        };

        await dialog.ShowAsync();
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
