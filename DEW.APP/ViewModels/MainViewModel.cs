using DEW.App.Helpers;
using DEW.App.Views;
using DEW.Core.Models;
using DEW.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace DEW.App.ViewModels;

//whenever changes are made, sends out signal and view redraws what is bound to it
public class MainViewModel : INotifyPropertyChanged
{
    // retireves JSON formatted entries
    public ObservableCollection<DialogueEntry> Entries { get; } = new();
    private string? _currentFilePath; // nullable, don't have to have a json loaded

    private bool _skipDeleteConfirm; //session-only skip for delete confirm

    private DialogueEntry? _cutBuffer; //holds cut content in context

    private bool CanPaste() => _cutBuffer != null; //makes sure there isn't nothing to paste

    private void Cut()
    {
        if (SelectedEntry == null) return; //can't cut nothing
        _cutBuffer = SelectedEntry; //grabs the selected entry and holds it in context
        RemoveSelectedAndSelectNeighbor();
    }

    private void Paste()
    {
        if (_cutBuffer == null) return;

        //paste below selection or at the end if nothing is selected
        int insertIndex = SelectedEntry !=  null
            ? Entries.IndexOf(SelectedEntry) + 1
            : Entries.Count;

        var duplicate = Entries.FirstOrDefault(e => e.Key == _cutBuffer.Key);
        if (duplicate != null)
        {
            if (!ConfirmOverwrite(_cutBuffer.Key)) return; //no = _cutBuffer continues to hold

            int dupIndex = Entries.IndexOf(duplicate);
            Entries.Remove(duplicate);
            if (dupIndex < insertIndex) insertIndex--; //list shrank above, shift target
        }

        Entries.Insert(insertIndex, _cutBuffer);
        SelectedEntry = _cutBuffer;
        _cutBuffer = null; //Empty the context
        CommandManager.InvalidateRequerySuggested();
    }

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

            CommandManager.InvalidateRequerySuggested(); //delete button grays/lights up as selection changes
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
    public ICommand DeleteCommand { get; } //delete the currently loaded entry
    public ICommand DuplicateCommand { get; }
    public ICommand CopyValueCommand { get; }
    public ICommand CutCommand { get; }
    public ICommand PasteCommand { get; }

    // draws content to the VM
    public MainViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
        SaveAsCommand = new RelayCommand(SaveAs, CanSave);
        LoadCommand = new RelayCommand(Load);
        NewCommand = new RelayCommand(New);
        CreateEntryCommand = new RelayCommand(CreateEntry);
        CreateEditEntryCommand = new RelayCommand(CreateEditEntry);
        DeleteCommand = new RelayCommand(Delete, HasSelection);
        DuplicateCommand = new RelayCommand(Duplicate, HasSelection);
        CopyValueCommand = new RelayCommand(CopyValue, HasSelection);
        CutCommand = new RelayCommand(Cut, HasSelection);
        PasteCommand = new RelayCommand(Paste, CanPaste);
    }

    private bool CanSave() => Entries.Count > 0; // when Entries are empty, lock saving

    private bool HasSelection() => SelectedEntry != null; //nothing selected, nothing to reference for Delete, Copy, etc.

    private bool ConfirmOverwrite(string key)
    {
        var result = MessageBox.Show(
                $"An entry with the key \"{key}\" already exists. This will overwrite it. Continue?",
                "Duplicate Key",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
    }

    private void RemoveSelectedAndSelectNeighbor()
    {
        if (SelectedEntry == null) return;

        int index = Entries.IndexOf(SelectedEntry);
        Entries.Remove(SelectedEntry);
        SelectedEntry = Entries.Count > 0 ? Entries[Math.Min(index, Entries.Count -1)] : null;
        CommandManager.InvalidateRequerySuggested();
    }

    private void Delete()
    {
        if (SelectedEntry == null) return;

        if (!_skipDeleteConfirm)
        {
            var dialog = new ConfirmDeleteDialog(SelectedEntry.Key)
            {
                Owner = Application.Current.MainWindow //centers popup over the main window
            };

            if(dialog.ShowDialog() != true) return; //cancel or esc: cancel delete command
            if (dialog.DontAskAgain) _skipDeleteConfirm = true; //if box checked, don't confirm delete for session only
        }

        RemoveSelectedAndSelectNeighbor();
    }

    public void CommitKeyChange()
    {
        if (SelectedEntry == null) return;
        if (EditingKey == SelectedEntry.Key) return;

        var duplicate = Entries.FirstOrDefault(e => e.Key == EditingKey && e != SelectedEntry);

        if (duplicate != null)
        {
            if (ConfirmOverwrite(EditingKey))
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

    private string GenerateUniqueKey(string baseKey = "NewEntry") // Keys must be unique. When generated, will append digits to prevent similar keys
    {
        //split "mon2" into stem "mon + number 2 (numebr part may be empty)
        var match = Regex.Match(baseKey, @"^(.?)(\d*)$");
        string stem = match.Groups[1].Value;
        int i = int.TryParse(match.Groups[2].Value, out int n) ? n + 1 : 1;

        string candidate = baseKey;
        while (Entries.Any(e => e.Key == candidate))
            candidate = $"{stem}{i++}";
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

    private void Duplicate()
    {
        if (SelectedEntry == null) return; //cancels if there isn't a selected entry

        var copy = new DialogueEntry
        {
            Key = GenerateUniqueKey(SelectedEntry.Key),
            RawText = SelectedEntry.RawText,
            IsNew = true //aut-generated key, same as Create
        };

        Entries.Insert(Entries.IndexOf(SelectedEntry) + 1, copy); //duplicates below the original
        SelectedEntry = copy;
        CommandManager.InvalidateRequerySuggested();
    }

    private void CopyValue()
    {
        if (SelectedEntry == null) return;
        Clipboard.SetText(SelectedEntry.RawText ?? string.Empty);
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