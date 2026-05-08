using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AppsLauncher;

public class LaunchItem : INotifyPropertyChanged
{
    private string _label = "New Item";
    private string _filePath = "";
    private string _parameters = "";
    private string _taskKillCommand = "";
    private string _status = "Idle";

    public string Label
    {
        get => _label;
        set { if (_label != value) { _label = value; OnPropertyChanged(); } }
    }

    public string FilePath
    {
        get => _filePath;
        set { if (_filePath != value) { _filePath = value; OnPropertyChanged(); } }
    }

    public string Parameters
    {
        get => _parameters;
        set { if (_parameters != value) { _parameters = value; OnPropertyChanged(); } }
    }

    public string TaskKillCommand
    {
        get => _taskKillCommand;
        set { if (_taskKillCommand != value) { _taskKillCommand = value; OnPropertyChanged(); } }
    }

    [JsonIgnore]
    public string Status
    {
        get => _status;
        set { if (_status != value) { _status = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
