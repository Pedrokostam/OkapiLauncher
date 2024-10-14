using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
namespace AuroraVisionLauncher.Controls;

public class AdornerContentPresenter : Adorner
{
    public RelativePlacement Placement { get; set; } = new();
    private readonly VisualCollection _Visuals;
    protected ContentPresenter ContentPresenter { get; private set; }    //public double ContentWidth
    //{
    //    get => _ContentPresenter.Width;
    //    set => _ContentPresenter.Width = value;
    //}
    //public double Height
    //{
    //    get => _ContentPresenter.Height;
    //    set => _ContentPresenter.Height = value;
    //}

    public AdornerContentPresenter(UIElement adornedElement)
      : base(adornedElement)
    {
        _Visuals = new VisualCollection(this);
        ContentPresenter = new ContentPresenter()
        {
            Content = new Border()
            {
                Background = Brushes.Transparent
            }
        };
        _Visuals.Add(ContentPresenter);

    }
    public Guid Guid { get; }


    public AdornerContentPresenter(UIElement adornedElement, UIElement content)
      : this(adornedElement)
    { Content = content; }

    protected override Size MeasureOverride(Size constraint)
    {
        var q = this.AdornedElement.DesiredSize;
        ContentPresenter.Measure(constraint);
        return ContentPresenter.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = Placement.GetRect(AdornedElement.RenderSize);
        Width = rect.Width;
        Height = rect.Height;
        ContentPresenter.Arrange(rect);
        return ContentPresenter.RenderSize;
    }

    protected override Visual GetVisualChild(int index) => _Visuals[index];

    protected override int VisualChildrenCount => _Visuals.Count;

    public UIElement Content
    {
        get => ((Border)ContentPresenter.Content).Child;
        set => ((Border)ContentPresenter.Content).Child = value;
    }
    new public object ToolTip
    {
        get => ((Border)ContentPresenter.Content).ToolTip;
        set => ((Border)ContentPresenter.Content).ToolTip = value;
    }

}

