using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ABI.System.Collections.Generic;
using Material.Icons.WPF;
namespace AuroraVisionLauncher.Controls;
/// <summary>
/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
///
/// Step 1a) Using this custom control in a XAML file that exists in the current project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:AuroraVisionLauncher.Controls"
///
///
/// Step 1b) Using this custom control in a XAML file that exists in a different project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:AuroraVisionLauncher.Controls;assembly=AuroraVisionLauncher.Controls"
///
/// You will also need to add a project reference from the project where the XAML file lives
/// to this project and Rebuild to avoid compilation errors:
///
///     Right click on the target project in the Solution Explorer and
///     "Add Reference"->"Projects"->[Browse to and select this project]
///
///
/// Step 2)
/// Go ahead and use your control in the XAML file.
///
///     <MyNamespace:WarningLaunchButton/>
///
/// </summary>
public class WarningLaunchButton : Button
{
    // Define a DependencyProperty for Count
    public static readonly DependencyProperty DisplayWarningProperty =
        DependencyProperty.Register(nameof(DisplayWarning), typeof(bool), typeof(WarningLaunchButton),
            new PropertyMetadata(false, OnDisplayWarningChanged));

    public bool DisplayWarning
    {
        get => (bool)GetValue(DisplayWarningProperty);
        set => SetValue(DisplayWarningProperty, value);
    }

    private static void OnDisplayWarningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WarningLaunchButton button)
        {
            button.UpdateAdorner();
        }
    }

    public WarningLaunchButton()
    {
        // Handle when the control is loaded to initialize the adorner
        Loaded += WarningLaunchButton_Loaded;
    }

    private void WarningLaunchButton_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateAdorner();
    }

    // Dynamically update the adorner based on the Count property
    private void UpdateAdorner()
    {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer is null)
        {
            return;
        }
        if (DisplayWarning && Visibility == Visibility.Visible)
        {
            if (adornerLayer.GetAdorners(this)?.FirstOrDefault() is null)
            {
                //var warningAdorner = new AdornerContentPresenter(this, new MaterialIcon() { Kind = Material.Icons.MaterialIconKind.Warning, ToolTip = "This app is already launched, you may not be able to launch another process", Foreground=Brushes.Yellow});
                var warningAdorner = new IconAdorner(this, Material.Icons.MaterialIconKind.Warning)
                {
                    ToolTip = "ksjdfnjksd",
                    Placement=new RelativePlacement(0,1,0.5,-0.5,DimensionType.Relative)
                };
                adornerLayer.Add(warningAdorner);
            }
        }
        else
        {
            var adorners = adornerLayer.GetAdorners(this);
            if (adorners is null)
            {
                return;
            }
            foreach (var adorner in adorners)
            {
                adornerLayer.Remove(adorner);
            }
        }
    }
}
public enum DimensionType
{
    Relative,
    Absolute,
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct TypedDimension
{
    public static readonly TypedDimension Zero = new TypedDimension(0, DimensionType.Relative);
    public static readonly TypedDimension Full = new TypedDimension(1, DimensionType.Relative);
    public double Value { get; }
    public DimensionType SizeType { get; }
    public TypedDimension()
    {
        Value = 1;
        SizeType = DimensionType.Relative;
    }
    public TypedDimension(double value, DimensionType sizeType)
    {
        Value = value;
        SizeType = sizeType;
    }
    public double GetDimension(double containerDimension)
    {
        return SizeType switch
        {
            DimensionType.Absolute => Value,
            DimensionType.Relative => Value * containerDimension,
            _ => throw new NotSupportedException(),
        };
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct RelativePlacement
{
    public TypedDimension X { get; }
    public TypedDimension Y { get; }
    public TypedDimension Width { get; }
    public TypedDimension Height { get; }
    public RelativePlacement() : this(
        TypedDimension.Zero,
        TypedDimension.Zero,
        TypedDimension.Full,
        TypedDimension.Full)
    { }

    public RelativePlacement(TypedDimension x, TypedDimension y, TypedDimension width, TypedDimension height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    public RelativePlacement(double x, double y, double width, double height, DimensionType commonDimension) : this(
        new TypedDimension(x, commonDimension),
        new TypedDimension(y, commonDimension),
        new TypedDimension(width, commonDimension),
        new TypedDimension(height, commonDimension))
    { }

    public Rect GetRect(double containingWidth, double containingHeight)
    {
        var originX = X.GetDimension(containingWidth);
        var originY = Y.GetDimension(containingHeight);
        var width = Width.GetDimension(containingWidth);
        var height = Height.GetDimension(containingHeight);

        if (width < 0)
        {
            width = -width;
            originX -= width;
        }
        if (height < 0)
        {
            height = -height;
            originY -= height;
        }
        return new Rect(originX, originY, width, height);

    }
    public Rect GetRect(Size containingSize)=>GetRect(containingSize.Width,containingSize.Height);
}
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


public class IconAdorner : AdornerContentPresenter
{
    public IconAdorner(UIElement adornedElement, Material.Icons.MaterialIconKind iconKind) : base(adornedElement, new MaterialIcon() { Kind = iconKind })
    {
    }
}

