using System.Windows;
using Material.Icons.WPF;
namespace AuroraVisionLauncher.Controls;

public class IconAdorner : AdornerContentPresenter
{
    public IconAdorner(UIElement adornedElement, Material.Icons.MaterialIconKind iconKind) : base(adornedElement, new MaterialIcon() { Kind = iconKind })
    {
    }
}

