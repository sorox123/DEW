using DEW.App.ViewModels;
using DEW.App.Helpers;
using DEW.Core;
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

    private void RawTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateHighlightPreview(RawTextBox.Text);
    }

    private void UpdateHighlightPreview(string rawText) //rebuild entire preview from scratch each time text changes
    {
        var document = new FlowDocument();
        var paragraph = new Paragraph();

        var tokens = DialogueTokenizer.Tokenize(rawText ?? string.Empty); //tokenize raw text

        foreach (var token in tokens) //for each token, create colored "run" (span of text, such as a word instead of painting whole sentence)
        {
            var run = new Run(token.RawText) { Foreground = Brushes.Black }; //colors text black
            run.Background = BackgroundForKind(token.Kind); //color every token based on kind

            var entry = SyntaxRegistry.Resolve(token.RawText);
            if (entry != null) //if entry isn't null, display info on json formatting
            {
                ToolTipService.SetToolTip(run, $"{entry.FriendlyName}\n{entry.Description}");
            }
            else if (token.Kind == SyntaxKind.Unknown) //if syntax is unknown, flag it and inform user via tooltip
            {
                run.ToolTip = "Unrecognized syntax - kept as-is.";
            }

            paragraph.Inlines.Add(run);
        }

        document.Blocks.Add(paragraph);
        HighlightPreview.Document = document;
    }

    private static Brush BackgroundForKind(SyntaxKind kind) //highlighting syntax based off of its kind
    {
        switch (kind)
        {
            case SyntaxKind.Inline: //if syntax is Inline (eg: name replacements), highlight blue
                return Brushes.LightBlue;
            case SyntaxKind.LineLevel: //if syntax is LineLevel (eg: portrait), highlight purple
                return Brushes.Plum;
            case SyntaxKind.Structural: //if syntax is Structural (eg: end conversation), highlight dark orange
                return Brushes.Moccasin;
            case SyntaxKind.Splitter: //if syntax is Splitter (eg: gender-based split), highlight dark cyan
                return Brushes.PaleTurquoise;
            case SyntaxKind.Portrait: //if syntax is Portrait, highlight palegreen
                return Brushes.PaleGreen;
            case SyntaxKind.Unknown:
                return Brushes.Gainsboro; //light highlighting to catch eye
            default:
                return Brushes.Transparent; //plain text, no highlight
        }
    }

    private void HighlightPreview_MouseMove(object sender, MouseEventArgs e)
    {
        var mousePos = e.GetPosition(HighlightPreview);
        var pointer = HighlightPreview.GetPositionFromPoint(mousePos, snapToText: true);

        if (pointer?.Parent is Run run && run.ToolTip is string tooltipText)
        {
            TokenTooltipText.Text = tooltipText;
            TokenTooltipPopup.HorizontalOffset = mousePos.X + 12;
            TokenTooltipPopup.VerticalOffset = mousePos.Y + 12;
            TokenTooltipPopup.IsOpen = true;
        }
        else
        {
            TokenTooltipPopup.IsOpen = false;
        }
    }

    private void HighlightPreview_MouseLeave(object sender, MouseEventArgs e)
    {
        TokenTooltipPopup.IsOpen = false;
    }
}