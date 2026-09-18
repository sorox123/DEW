using DEW.Core.Models;
using DEW.Core.Services;
using DEW.App.Helpers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
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
        get => _selectedEntry; //when selected entry changes, editingkey resets to match that entry's key
        set
        {
            _selectedEntry = value;
            EditingKey = value?.Key ?? string.Empty;
            OnPropertyChanged();
        }
    }

    private string _editingKey = string.Empty;
    public string EditingKey
    {
        get => _editingKey;
        set { _editingKey = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; } //saves
    public ICommand SaveAsCommand { get; } //saves As
    public ICommand LoadCommand { get; } //loads content
    public ICommand NewCommand { get; } //makes new json file, currently for dialogue files but might scope out further
    public ICommand CreateEntryCommand { get; set; } //create new entry for the file
    public ICommand CreateEditEntryCommand { get; set; } //create new entry and open it for editing

    // draws content to the VM
    public MainViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
        SaveAsCommand = new RelayCommand(SaveAs, CanSave);
        LoadCommand = new RelayCommand(Load);
        NewCommand = new RelayCommand(New);
        CreateEntryCommand = new RelayCommand(CreateEntry);
        CreateEditEntryCommand = new RelayCommand(CreateEditEntry);
    }

    private bool CanSave() => Entries.Count > 0; // when Entries are empty, lock saving

    public void CommitKeyChange()
    {
        if (SelectedEntry == null) return;
        if (EditingKey == SelectedEntry.Key) return;

        var duplicate = Entries.FirstOrDefault(e => e.Key == EditingKey && e != SelectedEntry);

        if (duplicate != null)
        {
            var result = MessageBox.Show(
                    $"An entry with the key \"{EditingKey}\" already exists. Saving will overwrite the entry. Continue?",
                    "Duplicate Key",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                Entries.Remove(duplicate);
                SelectedEntry.Key = EditingKey;
                SelectedEntry.IsNew = false;
            }
            //No -> leave EditingKey as typed but not saved, SelectedEntry.Key untouched
        }
        else
        {
            SelectedEntry.Key = EditingKey;
            SelectedEntry.IsNew = false;
        }
    }

    private void CreateEntry()
    {
        Entries.Add(new DialogueEntry { Key = GenerateUniqueKey(), IsNew = true }); // upon new entry, creates temporary key
        CommandManager.InvalidateRequerySuggested();
    }

    private void CreateEditEntry()
    {
        var entry = new DialogueEntry { Key = GenerateUniqueKey(), IsNew = true};
        Entries.Add(entry);
        SelectedEntry = entry;
        CommandManager.InvalidateRequerySuggested();
    }

    private string GenerateUniqueKey() // Keys must be unique. When generated, will append digits to prevent similar keys
    {
        int i = 1;
        string candidate = "NewEntry";
        while (Entries.Any(e => e.Key == candidate))
        {
            candidate = $"NewEntry{i++}";
        }
        return candidate;
    }

    private void New()
    {
        _currentFilePath = null;
        Entries.Clear();
        CommandManager.InvalidateRequerySuggested(); // makes Save/Save As grayed out and unclickable
    }

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