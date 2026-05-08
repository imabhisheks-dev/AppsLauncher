using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppsLauncher;

public class ClipItem : INotifyPropertyChanged
{
    private string _label = "New Snippet";
    private string _filePath = "";

    public string Label
    {
        get => _label;
        set { if (_label != value) { _label = value; OnPropertyChanged(); } }
    }

    /// <summary>
    /// Path to the file whose contents will be copied to the clipboard when the Copy button is clicked.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set { if (_filePath != value) { _filePath = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
