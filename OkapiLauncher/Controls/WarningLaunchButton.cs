using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
namespace OkapiLauncher.Controls;
/// <summary>
/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
///
/// Step 1a) Using this custom control in a XAML file that exists in the current project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:OkapiLauncher.Controls"
///
///
/// Step 1b) Using this custom control in a XAML file that exists in a different project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:OkapiLauncher.Controls;assembly=OkapiLauncher.Controls"
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
                var warningAdorner = new IconAdorner(this, Material.Icons.MaterialIconKind.Warning,Brushes.DarkOrange)
                {
                    ToolTip = Properties.Resources.WarningLanchButtonWarningTooltip,
                    //Placement = new RelativePlacement(0, 0, 1, 1, DimensionType.Relative,new Thickness(10)),
                    Placement = new RelativePlacement(0, 1, 0.5, -0.4, DimensionType.Relative, new Thickness(3,0,0,1)),
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
    public void SetIconKind(Material.Icons.MaterialIconKind kind)
    {
        ((Material.Icons.WPF.MaterialIcon)Content).Kind = kind;
    }
}

