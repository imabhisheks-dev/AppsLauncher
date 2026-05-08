using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppsLauncher;

public partial class ClipperWindow : Window
{
    private readonly ObservableCollection<ClipItem> _items = new();

    private static readonly string _sharedConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "launcher-config.json");

    public ClipperWindow()
    {
        InitializeComponent();
        ClipGrid.ItemsSource = _items;
        LoadConfig();

        if (_items.Count == 0)
            _items.Add(new ClipItem());
    }

    // ── Toolbar ──────────────────────────────────────────────────────────────

    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        var item = new ClipItem();
        _items.Add(item);
        ClipGrid.ScrollIntoView(item);
        ClipGrid.SelectedItem = item;
        SetStatus("Row added.");
    }

    private void RemoveRow_Click(object sender, RoutedEventArgs e)
    {
        if (ClipGrid.SelectedItem is ClipItem item)
        {
            _items.Remove(item);
            SetStatus($"Removed \"{item.Label}\".");
        }
        else
        {
            SetStatus("Select a row first.", isError: true);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveConfig(createBackup: true);
        SetStatus($"Saved {_items.Count} item(s).");
    }

    // ── Single-click editing ─────────────────────────────────────────────────

    private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var source = e.OriginalSource as DependencyObject;
        while (source != null && source is not DataGridCell)
            source = System.Windows.Media.VisualTreeHelper.GetParent(source);

        if (source is DataGridCell { IsEditing: false, IsReadOnly: false } cell)
        {
            if (e.OriginalSource is not Button &&
                FindAncestor<Button>(e.OriginalSource as DependencyObject) == null)
            {
                cell.Focus();
                ClipGrid.BeginEdit(e);
            }
        }
    }

    // ── Copy button ──────────────────────────────────────────────────────────

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ClipItem item }) return;

        if (string.IsNullOrWhiteSpace(item.FilePath))
        {
            SetStatus($"[{item.Label}] No source file configured.", isError: true);
            return;
        }

        if (!File.Exists(item.FilePath))
        {
            SetStatus($"[{item.Label}] File not found: {item.FilePath}", isError: true);
            return;
        }

        try
        {
            var text = File.ReadAllText(item.FilePath);
            Clipboard.SetText(text);
            SetStatus($"Copied \"{item.Label}\" ({text.Length:N0} chars) to clipboard.");
        }
        catch (Exception ex)
        {
            SetStatus($"[{item.Label}] Copy failed: {ex.Message}", isError: true);
        }
    }

    // ── Config persistence ───────────────────────────────────────────────────

    private static AppConfig ReadSharedConfig()
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
            var section = ReadSharedConfig().Clipper;

            if (section.WindowWidth  >= MinWidth)  Width  = section.WindowWidth;
            if (section.WindowHeight >= MinHeight) Height = section.WindowHeight;

            foreach (var item in section.Items)
                _items.Add(item);

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

            // Read the full config so we preserve the Launcher section
            var config = ReadSharedConfig();
            config.Clipper = new AppConfig.ClipperSection
            {
                WindowWidth  = Width,
                WindowHeight = Height,
                Items        = _items.ToList()
            };

            var json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(_sharedConfigPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save:\n{ex.Message}",
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
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
            if (obj is T t) return t;
            obj = System.Windows.Media.VisualTreeHelper.GetParent(obj);
        }
        return null;
    }
}
