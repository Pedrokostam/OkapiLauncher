using System.Runtime.InteropServices;
namespace AuroraVisionLauncher.Controls;

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

