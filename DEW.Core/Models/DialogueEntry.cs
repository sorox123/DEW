using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DEW.Core.Models;

//blueprint for dialogue entry
public class DialogueEntry : INotifyPropertyChanged
{
    private string _key = string.Empty;
    public string Key
    {
        get => _key;
        set { _key = value; OnPropertyChanged(); }
    }

    private string _rawText = string.Empty;
    public string RawText
    {
        get => _rawText;
        set { _rawText = value; OnPropertyChanged(); }
    }

    public bool IsNew { get; set; } = false; // entry just created and never had a key confirmed yet?

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}