using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppsLauncher;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<LaunchItem> _items = new();
    private readonly CancellationTokenSource _pollCts = new();
    private bool _editingEnabled = false;  // Application / Program/Script editing starts locked
    private ClipperWindow? _clipperWindow;

    public MainWindow()
    {
        InitializeComponent();
        LaunchGrid.ItemsSource = _items;
        LoadConfig();

        if (_items.Count == 0)
            _items.Add(new LaunchItem());

        ApplyEditLock();
        StartPolling();
    }

    // ── Process status polling ───────────────────────────────────────────────

    private void StartPolling()
    {
        var token = _pollCts.Token;
        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(token))
                await Dispatcher.InvokeAsync(PollProcessStatuses);
        }, token);
    }

    private void PollProcessStatuses()
    {
        foreach (var item in _items)
        {
            if (string.IsNullOrWhiteSpace(item.TaskKillCommand)) continue;

            var processName = ParseProcessName(item.TaskKillCommand);
            if (string.IsNullOrWhiteSpace(processName)) continue;

            var procs = Process.GetProcessesByName(processName);
            item.Status = procs.Length > 0 ? "Running" : "Stopped";
            foreach (var p in procs) p.Dispose();
        }
    }

    private static string ParseProcessName(string taskKillCommand)
    {
        var raw = taskKillCommand.Trim();

        // Try to extract the image name from /IM <name> argument
        var match = Regex.Match(raw, @"/IM\s+(\S+)", RegexOptions.IgnoreCase);
        var imageName = match.Success
            ? match.Groups[1].Value
            : raw.StartsWith("/", StringComparison.Ordinal) || raw.StartsWith("taskkill", StringComparison.OrdinalIgnoreCase)
                ? string.Empty                // can't determine process name
                : raw.Split(' ')[0];          // bare name like "notepad.exe" or "notepad"

        return string.IsNullOrWhiteSpace(imageName)
            ? string.Empty
            : Path.GetFileNameWithoutExtension(imageName);
    }

    // ── Toolbar handlers ────────────────────────────────────────────────────

    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        var item = new LaunchItem();
        _items.Add(item);
        LaunchGrid.ScrollIntoView(item);
        LaunchGrid.SelectedItem = item;
        SetStatus("Row added. Double-click Label or Parameters to edit.");
    }

    private void RemoveRow_Click(object sender, RoutedEventArgs e)
    {
        if (LaunchGrid.SelectedItem is LaunchItem item)
        {
            var name = item.Label;
            _items.Remove(item);
            SetStatus($"Removed \"{name}\".", isError: false);
        }
        else
        {
            SetStatus("Select a row first, then click Remove.", isError: true);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveConfig(createBackup: true);
        SetStatus($"Saved {_items.Count} item(s) to {_sharedConfigPath}");
    }

    private void EditToggle_Click(object sender, RoutedEventArgs e)
    {
        _editingEnabled = !_editingEnabled;
        ApplyEditLock();
        SetStatus(_editingEnabled
            ? "Editing enabled — Program/Script paths are now editable."
            : "Editing locked — Program/Script paths are read-only.");
    }

    private void ApplyEditLock()
    {
        ColApplication.IsReadOnly  = !_editingEnabled;
        ColProgramScript.IsReadOnly = !_editingEnabled;
        EditToggleBtn.Content = _editingEnabled ? "\uD83D\uDD13 Editing: ON" : "\uD83D\uDD12 Editing: OFF";
        // Amber when locked, green when unlocked
        EditToggleBtn.Background = _editingEnabled
            ? System.Windows.Media.Brushes.SeaGreen
            : System.Windows.Media.Brushes.DarkGoldenrod;
    }

    private void OpenClipper_Click(object sender, RoutedEventArgs e)
    {
        if (_clipperWindow is null || !_clipperWindow.IsLoaded)
        {
            _clipperWindow = new ClipperWindow { Owner = this };
            _clipperWindow.Show();
        }
        else
        {
            _clipperWindow.Activate();
        }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        var readme = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "README.md"));
        if (File.Exists(readme))
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(readme) { UseShellExecute = true });
        else
            SetStatus("README.md not found at: " + readme, isError: true);
    }

    // ── DataGrid single-click editing ────────────────────────────────────────

    /// <summary>
    /// Allows single-click to enter edit mode in Label / Parameters / FilePath cells.
    /// Without this, WPF DataGrid requires a double-click.
    /// </summary>
    private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Walk up the visual tree from the clicked element to find the DataGridCell
        var source = e.OriginalSource as DependencyObject;
        while (source != null && source is not DataGridCell)
            source = System.Windows.Media.VisualTreeHelper.GetParent(source);

        if (source is DataGridCell { IsEditing: false, IsReadOnly: false } cell)
        {
            // Don't enter edit mode when the click lands directly on a Button
            // (Browse / Launch) — their Click handlers will fire independently.
            if (e.OriginalSource is not Button &&
                FindAncestor<Button>(e.OriginalSource as DependencyObject) == null)
            {
                cell.Focus();
                LaunchGrid.BeginEdit(e);
            }
        }
    }

    // ── Cell handlers ────────────────────────────────────────────────────────

    private void Kill_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: LaunchItem item }) return;

        if (string.IsNullOrWhiteSpace(item.TaskKillCommand))
        {
            SetStatus($"[{item.Label}] No TaskKill command configured.", isError: true);
            MessageBox.Show(
                "Please enter a taskkill command, e.g.:\n  taskkill /IM notepad.exe /F",
                "No TaskKill Command",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Allow the user to type either the full command (taskkill /IM foo.exe /F)
        // or just the arguments (/IM foo.exe /F) or just a process name (notepad.exe).
        var raw = item.TaskKillCommand.Trim();
        string fileName;
        string arguments;

        if (raw.StartsWith("taskkill", StringComparison.OrdinalIgnoreCase))
        {
            // Full command: "taskkill /IM notepad.exe /F"
            var spaceIdx = raw.IndexOf(' ');
            fileName  = spaceIdx < 0 ? raw : raw[..spaceIdx];
            arguments = spaceIdx < 0 ? "" : raw[(spaceIdx + 1)..];
        }
        else if (raw.StartsWith("/", StringComparison.Ordinal))
        {
            // Just args: "/IM notepad.exe /F"
            fileName  = "taskkill";
            arguments = raw;
        }
        else
        {
            // Bare process name: "notepad.exe"
            fileName  = "taskkill";
            arguments = $"/IM {raw} /F";
        }

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName               = fileName,
                Arguments              = arguments,
                UseShellExecute        = false,
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            using var proc = System.Diagnostics.Process.Start(psi);
            proc?.WaitForExit(3000);
            var output = proc?.StandardOutput.ReadToEnd().Trim();
            var error  = proc?.StandardError.ReadToEnd().Trim();

            if (!string.IsNullOrEmpty(error))
            {
                item.Status = "Error";
                SetStatus($"[{item.Label}] Kill: {error}", isError: true);
            }
            else
            {
                item.Status = "Terminated";
                SetStatus($"[{item.Label}] Kill: {(string.IsNullOrEmpty(output) ? "command sent." : output)}");
            }
        }
        catch (Exception ex)
        {
            item.Status = "Error";
            var msg = $"Failed to run taskkill for \"{item.Label}\":\n{ex.Message}";
            SetStatus(msg, isError: true);
            MessageBox.Show(msg, "Kill Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Launch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: LaunchItem item }) return;

        if (string.IsNullOrWhiteSpace(item.FilePath))
        {
            SetStatus($"[{item.Label}] No file configured.", isError: true);
            MessageBox.Show(
                "Please select a file to launch using the Browse button.",
                "No File Selected",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var psi = BuildProcessStartInfo(item);
            Process.Start(psi);
            item.Status = "Running";
            SetStatus($"Launched: {item.Label}");
        }
        catch (Exception ex)
        {
            item.Status = "Error";
            var msg = $"Failed to launch \"{item.Label}\":\n{ex.Message}";
            SetStatus(msg, isError: true);
            MessageBox.Show(msg, "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Process builder ──────────────────────────────────────────────────────

    private static ProcessStartInfo BuildProcessStartInfo(LaunchItem item)
    {
        var ext = Path.GetExtension(item.FilePath).ToLowerInvariant();
        var args = item.Parameters ?? string.Empty;

        return ext switch
        {
            ".ps1" => new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{item.FilePath}\" {args}",
                UseShellExecute = true
            },
            ".py" => new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{item.FilePath}\" {args}",
                UseShellExecute = true
            },
            _ => new ProcessStartInfo
            {
                FileName = item.FilePath,
                Arguments = args,
                UseShellExecute = true
            }
        };
    }

    // ── Config persistence ───────────────────────────────────────────────────

    private static readonly string _sharedConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "launcher-config.json");

    private static AppConfig ReadConfig()
    {
        if (!File.Exists(_sharedConfigPath)) return new AppConfig();
        try
        {
            var json = File.ReadAllText(_sharedConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch { return new AppConfig(); }
    }

    private void LoadConfig()
    {
        try
        {
            var config = ReadConfig();
            var section = config.Launcher;

            if (section.WindowWidth  >= MinWidth)  Width  = section.WindowWidth;
            if (section.WindowHeight >= MinHeight) Height = section.WindowHeight;

            // Restore saved column widths
            var colMap = new Dictionary<string, DataGridTemplateColumn>
            {
                ["Application"]   = ColApplication,
                ["ProgramScript"] = ColProgramScript,
                ["Launch"]        = ColLaunch,
                ["Status"]        = ColStatus,
                ["TaskKill"]      = ColTaskKill,
            };
            foreach (var (key, col) in colMap)
                if (section.ColumnWidths.TryGetValue(key, out var w) && w >= 20)
                    col.Width = new DataGridLength(w);

            foreach (var item in section.Items)
            {
                item.Status = "Idle";
                _items.Add(item);
            }

            if (_items.Count > 0)
                SetStatus($"Loaded {_items.Count} item(s).");
        }
        catch (Exception ex)
        {
            SetStatus($"Could not load config: {ex.Message}", isError: true);
        }
    }

    private void SaveConfig(bool createBackup = false)
    {
        try
        {
            if (createBackup && File.Exists(_sharedConfigPath))
            {
                var timestamp  = DateTime.Now.ToString("yyyyMMddTHHmmss");
                var backupPath = Path.Combine(
                    Path.GetDirectoryName(_sharedConfigPath)!,
                    $"launcher-config_{timestamp}.json");
                File.Copy(_sharedConfigPath, backupPath, overwrite: false);
            }

            // Read the full config so we preserve the Clipper section
            var config = ReadConfig();
            config.Launcher = new AppConfig.LauncherSection
            {
                WindowWidth  = Width,
                WindowHeight = Height,
                ColumnWidths = new Dictionary<string, double>
                {
                    ["Application"]   = ColApplication.ActualWidth,
                    ["ProgramScript"] = ColProgramScript.ActualWidth,
                    ["Launch"]        = ColLaunch.ActualWidth,
                    ["Status"]        = ColStatus.ActualWidth,
                    ["TaskKill"]      = ColTaskKill.ActualWidth,
                },
                Items = _items.ToList()
            };

            var json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(_sharedConfigPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save configuration:\n{ex.Message}",
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _pollCts.Cancel();
        SaveConfig();
        base.OnClosing(e);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetStatus(string message, bool isError = false)
    {
        StatusText.Text = message;
        StatusIcon.Text = isError ? "⚠" : "✓";
        StatusIcon.Foreground = isError
            ? System.Windows.Media.Brushes.DarkOrange
            : System.Windows.Media.Brushes.Green;
    }

    private static T? FindAncestor<T>(DependencyObject? obj) where T : DependencyObject
    {
        while (obj != null)
        {
            if (obj is T target) return target;
            obj = System.Windows.Media.VisualTreeHelper.GetParent(obj);
        }
        return null;
    }
}
