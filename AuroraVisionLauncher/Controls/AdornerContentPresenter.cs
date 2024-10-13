using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
namespace AuroraVisionLauncher.Controls;

public class AdornerContentPresenter : Adorner
{
    public RelativePlacement Placement { get; set; } = new();
    private VisualCollection _Visuals;
    private ContentPresenter _ContentPresenter;
    //public double ContentWidth
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
        _ContentPresenter = new ContentPresenter()
        {
            Content = new Border()
            {
                Background = Brushes.Transparent
            }
        };
        _Visuals.Add(_ContentPresenter);

    }
    public Guid Guid { get; }


    public AdornerContentPresenter(UIElement adornedElement, UIElement content)
      : this(adornedElement)
    { Content = content; }

    protected override Size MeasureOverride(Size constraint)
    {
        var q = this.AdornedElement.DesiredSize;
        _ContentPresenter.Measure(constraint);
        return _ContentPresenter.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = Placement.GetRect(AdornedElement.RenderSize);
        Width = rect.Width;
        Height = rect.Height;
        _ContentPresenter.Arrange(rect);
        return _ContentPresenter.RenderSize;
    }

    protected override Visual GetVisualChild(int index) => _Visuals[index];

    protected override int VisualChildrenCount => _Visuals.Count;

    public UIElement Content
    {
        get => ((Border)_ContentPresenter.Content).Child;
        set => ((Border)_ContentPresenter.Content).Child = value;
    }
    new public object ToolTip
    {
        get => ((Border)_ContentPresenter.Content).ToolTip;
        set => ((Border)_ContentPresenter.Content).ToolTip = value;
    }

}

