using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Material.Icons.WPF;
namespace AuroraVisionLauncher.Controls;

public class IconAdorner : AdornerContentPresenter
{
    public IconAdorner(UIElement adornedElement, Material.Icons.MaterialIconKind iconKind,Brush? foreground=null) : base(adornedElement)
    {
        var icon = new MaterialIcon()
        {
            Kind = iconKind,
        };
        if(foreground is not null)
        {
            icon.Foreground = foreground;
        }
        Content = icon;
    }
}

