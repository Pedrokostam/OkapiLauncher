using System.Runtime.InteropServices;
using System.Windows;
namespace AuroraVisionLauncher.Controls;

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

