namespace AppsLauncher;

/// <summary>
/// Unified on-disk configuration stored in launcher-config.json.
/// </summary>
public sealed class AppConfig
{
    public LauncherSection Launcher { get; set; } = new();
    public ClipperSection  Clipper  { get; set; } = new();

    public sealed class LauncherSection
    {
        public double WindowWidth  { get; set; } = 900;
        public double WindowHeight { get; set; } = 480;
        public Dictionary<string, double> ColumnWidths { get; set; } = new();
        public List<LaunchItem> Items { get; set; } = new();
    }

    public sealed class ClipperSection
    {
        public double WindowWidth  { get; set; } = 660;
        public double WindowHeight { get; set; } = 420;
        public List<ClipItem> Items { get; set; } = new();
    }
}
