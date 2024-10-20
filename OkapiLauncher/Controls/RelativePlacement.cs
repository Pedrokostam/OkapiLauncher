using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
namespace OkapiLauncher.Controls;

[StructLayout(LayoutKind.Auto)]
public readonly record struct RelativePlacement
{
    public TypedDimension X { get; }
    public TypedDimension Y { get; }
    public TypedDimension Width { get; }
    public TypedDimension Height { get; }
    public Thickness Padding { get; }
    public RelativePlacement() : this(
        TypedDimension.Zero,
        TypedDimension.Zero,
        TypedDimension.Full,
        TypedDimension.Full)
    { }

    public RelativePlacement(TypedDimension x, TypedDimension y, TypedDimension width, TypedDimension height):this(x, y, width, height, new Thickness(0)) { }
    public RelativePlacement(TypedDimension x, TypedDimension y, TypedDimension width, TypedDimension height, Thickness padding)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Padding = padding;
    }
    public RelativePlacement(double x, double y, double width, double height, DimensionType commonDimension) : this(x,y,width,height,commonDimension,new Thickness(0)) { }
    public RelativePlacement(double x, double y, double width, double height, DimensionType commonDimension, Thickness padding) : this(
        new TypedDimension(x, commonDimension),
        new TypedDimension(y, commonDimension),
        new TypedDimension(width, commonDimension),
        new TypedDimension(height, commonDimension),
        padding)
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
        originX += Padding.Left;
        originY+= Padding.Top;
        width -= Padding.Left + Padding.Right;
        height -= Padding.Top + Padding.Bottom;
        width = Math.Clamp(width, 0, double.MaxValue);
        height = Math.Clamp(height, 0, double.MaxValue);
        return new Rect(originX, originY, width, height);

    }
    public Rect GetRect(Size containingSize)=>GetRect(containingSize.Width,containingSize.Height);
}

