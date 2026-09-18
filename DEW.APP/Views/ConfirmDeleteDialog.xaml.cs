using System.Windows;

namespace DEW.App.Views;

public partial class ConfirmDeleteDialog : Window
{
    //allows caller to read the checkbox after dialog closes
    public bool DontAskAgain => DontAskCheckBox.IsChecked == true;

    public ConfirmDeleteDialog(string entryKey)
    {
        InitializeComponent();
        MessageText.Text = $"Delete \"{entryKey}\"? This can't be undone.";
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true; //closes window, ShowDialog returns true
    }
}