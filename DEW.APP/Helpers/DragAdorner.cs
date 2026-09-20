using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DEW.App.Helpers;

public class DragAdorner : Adorner
{
    private readonly Rectangle _child;
    private double _offsetX;
    private double _offsetY;

    public DragAdorner(UIElement adornedElement, UIElement dragged) : base(adornedElement)
    {
        var brush = new VisualBrush(dragged) { Opacity = 0.7 };
        _child = new Rectangle
        {
            Width = dragged.RenderSize.Width,
            Height = dragged.RenderSize.Height,
            Fill = brush
        };
        IsHitTestVisible = false; //never incercepts mouse events
    }

    public void UpdatePosition(double left, double top)
    {
        _offsetX = left;
        _offsetY = top;
        (Parent as AdornerLayer)?.Update(AdornedElement);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _child.Arrange(new Rect(new Point(0, 0), new Size(_child.Width, _child.Height)));
        return finalSize;
    }

    protected override Visual GetVisualChild(int index) => _child;
    protected override int VisualChildrenCount => 1;

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
        var group = new GeneralTransformGroup();
        group.Children.Add(base.GetDesiredTransform(transform));
        group.Children.Add(new TranslateTransform(_offsetX, _offsetY));
        return group;
    }
}