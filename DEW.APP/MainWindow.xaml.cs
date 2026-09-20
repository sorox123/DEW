using DEW.App.ViewModels;
using DEW.App.Helpers;
using DEW.Core.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace DEW.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void KeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.CommitKeyChange();
        }
    }

    private void ListBoxItem_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem item)
        {
            item.Focus(); //first, so half-typed key commits. Acts like left-click
            item.IsSelected = true;
        }
    }

    private Point _dragStartPoint;
    private DragAdorner? _dragAdorner;
    private AdornerLayer? _adornerLayer;

    private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (sender is not ListBoxItem item) return;

        Point currentPosition = e.GetPosition(null);
        Vector diff = _dragStartPoint - currentPosition;

        bool pastThreshold =
            Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance;

        if (!pastThreshold) return;

        if (item.DataContext is DialogueEntry entry)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(EntriesListBox);
            _dragAdorner = new DragAdorner(EntriesListBox, item);
            _adornerLayer.Add(_dragAdorner);

            DragDrop.DoDragDrop(item, entry, DragDropEffects.Move);

            _adornerLayer.Remove(_dragAdorner);
            _dragAdorner = null;
        }
    }

    private void ListBox_DragOver(object sender, DragEventArgs e)
    {
        if (_dragAdorner == null) return;
        Point position = e.GetPosition(EntriesListBox);
        _dragAdorner.UpdatePosition(position.X + 10, position.Y +1);
    }

    private void ListBox_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (!e.Data.GetDataPresent(typeof(DialogueEntry))) return;

        var droppedEntry = (DialogueEntry)e.Data.GetData(typeof(DialogueEntry))!;

        //find what item the mous is over in ListBox (if there is any)
        var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        int targetIndex = targetItem?.DataContext is DialogueEntry targetEntry
            ? vm.Entries.IndexOf(targetEntry)
            : vm.Entries.Count - 1; //if dropped on empty space, move to the end of the list

        int sourceIndex = vm.Entries.IndexOf(droppedEntry);
        vm.MoveEntry(sourceIndex, targetIndex);
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm && !vm.ConfirmDiscardChanges())
        {
            e.Cancel = true;
        }
    }
}