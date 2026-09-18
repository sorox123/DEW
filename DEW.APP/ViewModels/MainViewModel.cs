using DEW.Core.Models;
using DEW.Core.Services;
using DEW.App.Helpers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DEW.App.ViewModels;

//whenever changes are made, sends out signal and view redraws what is bound to it
public class MainViewModel : INotifyPropertyChanged
{
    // retireves JSON formatted entries
    public ObservableCollection<DialogueEntry> Entries { get; } = new();
    private string? _currentFilePath; // nullable, don't have to have a json loaded

    // makes DialogueEntry nullable so it doesn't crash if nothing is selected
    private DialogueEntry? _selectedEntry;
    public DialogueEntry? SelectedEntry
    {
        get => _selectedEntry;
        set { _selectedEntry = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand LoadCommand { get; }

    // draws content to the VM
    public MainViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
        SaveAsCommand = new RelayCommand(SaveAs, CanSave);
        LoadCommand = new RelayCommand(Load);
    }

    private bool CanSave() => Entries.Count > 0; // when Entries are empty, lock saving

    private void LoadFile(string path)
    {
        _currentFilePath = path;
        Entries.Clear(); // clears any Entries before loading new Entries
        foreach (var entry in DialogueFileLoader.Load(path))
        {
            Entries.Add(entry); // appends dialogue entries to Entries for VM
        }
        CommandManager.InvalidateRequerySuggested(); //recheck every command's CanExecute, without this, buttons would immediately become clickable
    }

    private void Load()
    {
        var dialog = new OpenFileDialog //Opens files with attached filter
        {
            Filter = "JSON files (*.json)|*.json"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadFile(dialog.FileName);
        }
    }

    private void Save()
    {
        if (_currentFilePath is null) return; // if somehow able to save, button does nothing
        DialogueFileSaver.Save(Entries.ToList(), _currentFilePath);
    }

    private void SaveAs()
    {
        //builds dialog window settings before showing it
        var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            FileName = _currentFilePath is not null ? Path.GetFileName(_currentFilePath) : "dialogue.json"
        };

        if (dialog.ShowDialog() == true) //checks to make sure the user presses save instead of doing something else
        {
            //saves entries (and changes) to file at _currentFilePath
            _currentFilePath = dialog.FileName;
            DialogueFileSaver.Save(Entries.ToList(), _currentFilePath);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

}