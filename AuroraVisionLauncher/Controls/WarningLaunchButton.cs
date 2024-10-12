using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        if (DisplayWarning &&  Visibility==Visibility.Visible)
        {
            if (adornerLayer.GetAdorners(this)?.FirstOrDefault() is null)
            {
                var warningAdorner = new AdornerContentPresenter(this, new MaterialIcon() { Kind = Material.Icons.MaterialIconKind.Warning, ToolTip = "This app is already launched, you may not be able to launch another process", Foreground=Brushes.Yellow});
                //var warningAdorner = new WarningAdorner(this);
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
public class AdornerContentPresenter : Adorner
{
    private VisualCollection _Visuals;
    private ContentPresenter _ContentPresenter;

    public AdornerContentPresenter(UIElement adornedElement)
      : base(adornedElement)
    {
        _Visuals = new VisualCollection(this);
        _ContentPresenter = new ContentPresenter() { Width=18, Height=18};
        _Visuals.Add(_ContentPresenter);
    }

    public AdornerContentPresenter(UIElement adornedElement, Visual content)
      : this(adornedElement)
    { Content = content; }

    protected override Size MeasureOverride(Size constraint)
    {
        _ContentPresenter.Measure(constraint);
        return _ContentPresenter.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _ContentPresenter.Arrange(new Rect(0, 0,
             finalSize.Width, finalSize.Height));
        return _ContentPresenter.RenderSize;
    }

    protected override Visual GetVisualChild(int index)
    { return _Visuals[index]; }

    protected override int VisualChildrenCount
    { get { return _Visuals.Count; } }

    public object Content
    {
        get { return _ContentPresenter.Content; }
        set { _ContentPresenter.Content = value; }
    }
}



public class WarningAdorner : Adorner
{
    private readonly UIElement _child;

    public WarningAdorner(UIElement adornedElement) : base(adornedElement)
    {
        var icon = new MaterialIcon()
        {
            Kind = Material.Icons.MaterialIconKind.WarningBoxOutline,
            Foreground=Brushes.Red,
        };

        //var box = new Viewbox()
        //{

        //    Child = icon,
        //};
        //var border = new Border()
        //{
        //    HorizontalAlignment = HorizontalAlignment.Stretch,
        //    VerticalAlignment = VerticalAlignment.Stretch,
        //    Background = Brushes.Gray,
        //    BorderThickness=new Thickness(2),
        //    BorderBrush=Brushes.Green,
        //    Child = icon,
        //    ToolTip="skdjfnjksdnfsdj",
        //};
        _child = new Border()
        {
            
            Child = icon,
            ToolTip="fsdf",
            Background=Brushes.Transparent,
            Width = 20,
            Height = 20
        };
        //_child = new Viewbox
        //{
        //    Stretch = Stretch.UniformToFill,
        //    HorizontalAlignment = HorizontalAlignment.Right,
        //    VerticalAlignment = VerticalAlignment.Bottom,
        //    Child = new MaterialIcon
        //    {
        //        Kind = Material.Icons.MaterialIconKind.Star,
        //        Foreground = Brushes.Red,
        //        FontSize = 20,
        //    }
        //};
        //_child = new MaterialIcon
        //{
        //    Kind = Material.Icons.MaterialIconKind.Star,
        //    Foreground = Brushes.Red,
        //    Width = 15,
        //    Height = 15,
        //};
        //    _child = new Border
        //    {
        //        //Width = adornedElement.RenderSize.Width / 2,  // Occupy half the width
        //        //Height = adornedElement.RenderSize.Height / 2,  // Occupy half the height
        //        Background = Brushes.Cyan,  // Transparent but clickable
        //        ToolTip = new ToolTip
        //        {
        //            Content = "Bazingo",
        //            Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
        //        },
        //        //Child= new TextBlock
        //        //{
        //        //               Text = "⚠️",
        //        //               Foreground = Brushes.Yellow,
        //        //               FontSize = 20,
        //        //               ToolTip = "This app is already launched, you may not be able to launch another process"
        //        //           },
        //
        //        Child = new MaterialIcon
        //        {
        //            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
        //            VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
        //            VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch,
        //            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
        //            Kind = Material.Icons.MaterialIconKind.Star,
        //            Foreground = Brushes.Red,
        //            //Width=15,
        //            //Height=15,
        //        }
        //    };
        //Create the warning icon(e.g., ⚠️) and tooltip
        //_child = new Rectangle() { Fill = Brushes.Green };
        //_child = new TextBlock
        //              {
        //                  Text = "⚠️",
        //                  Foreground = Brushes.Yellow,
        //                  FontSize = 20,
        //                  ToolTip = "This app is already launched, you may not be able to launch another process"
        //              };
    }

    protected override Visual GetVisualChild(int index) => _child;

    protected override int VisualChildrenCount => 1;

    protected override Size MeasureOverride(Size constraint)
    {
        _child.Measure(constraint);
        return _child.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        //_child.RenderSize = new Size(finalSize.Width / 2, finalSize.Height / 2);
        _child.Arrange(
            new Rect(
                x: 0,
                y: 0,
                width: finalSize.Width,
                height: finalSize.Height
                ));
        //_child.Arrange(new Rect(finalSize.Width / 2, finalSize.Height / 2, finalSize.Width / 2, finalSize.Height / 2));
        //_child.Arrange(new Rect(new Point(0, finalSize.Height), _child.DesiredSize));  // Position the adorner
        return _child.RenderSize;
    }
}

