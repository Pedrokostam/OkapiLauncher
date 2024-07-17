using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace AuroraVisionLauncher.Core.Models;
public enum AvType
{
    Professional,
    Runtime,
    DeepLearning,
    Library
}
public class ProductType : IComparable<ProductType>, IComparable
{
    public static readonly ProductType Professional = new("Professional", AvType.Professional);
    public static readonly ProductType Runtime = new("Runtime", AvType.Runtime);
    public static readonly ProductType DeepLearning = new("DeepLearning", AvType.DeepLearning);
    public static readonly ProductType Library = new("Library", AvType.Library);
    private static readonly ProductType[] _types = [Professional, Runtime, DeepLearning, Library];
    public string Name { get; }
    private readonly List<ProductType> _supportedAvTypes = [];
    public AvType Type { get; }
    private string ProductNameKeyword { get; }
    private string? FileSignatureKeyword { get; }
    public IReadOnlyCollection<ProductType> SupportedAvTypes => _supportedAvTypes.AsReadOnly();

    public static ProductType FromAvType(AvType type)
    {
        return type switch
        {
            AvType.Professional => Professional,
            AvType.Runtime => Runtime,
            AvType.DeepLearning => DeepLearning,
            AvType.Library => Library,
            _ => throw new NotSupportedException()
        };
    }

    private ProductType(string name, AvType type)
    {
        Name = name;
        Type = type;
    }


    static ProductType()
    {
        Professional._supportedAvTypes.Add(Professional);

        Runtime._supportedAvTypes.Add(Runtime);
        Runtime._supportedAvTypes.Add(Professional);

        DeepLearning._supportedAvTypes.Add(DeepLearning);

        Library._supportedAvTypes.Add(Library);
    }

    public bool SupportsType(ProductType type)
    {
        return _supportedAvTypes.Contains(type);
    }

    public int CompareTo(ProductType? other)
    {
        if (other is null)
        {
            return 1;
        }
        return Type.CompareTo(other.Type);
    }
    public override string ToString() => Name;

    public int CompareTo(object? obj) => CompareTo(obj as ProductType);
}