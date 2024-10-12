using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Core.Exceptions;
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
    public string Name { get; }
    private readonly List<ProductType> _supportedAvTypes = [];
    public AvType Type { get; }
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

    /// <summary>
    /// Finds the path to the root folder.
    /// </summary>
    /// <param name="filepath">Path to the executable file of this type</param>
    /// <returns>Path to the root folder accoring to the filepath and type</returns>
    public string GetRootFolder(string filepath)
    {
        // the same thing tat is calculated in PathStem, but larger by 1
        // Can possibly avoid code duplication?
        int steps = this.Type switch
        {
            AvType.Professional => 1,
            AvType.Runtime => 1,
            AvType.DeepLearning => 3,
            AvType.Library => 3,
            _ => throw new NotSupportedException(),
        };
        string[] dots = new string[steps];
        Array.Fill(dots, "..");
        string ladder = string.Join(Path.DirectorySeparatorChar, dots);
        return Path.GetFullPath(Path.Combine(filepath, ladder));
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
    /// <summary>
    /// Find matching <see cref="AvType"/> based on the filename.
    /// </summary>
    /// <param name="filepath">Path to a file, or its name.</param>
    /// <returns>Matching <see cref="AvType"/></returns>
    /// <exception cref="InvalidAppTypeNameException"></exception>
    public static AvType GetAvTypeFromFilename(string filepath)
    {
        string name = Path.GetFileName(filepath).ToLowerInvariant();
        return name switch
        {
            "adaptivevisionstudio.exe" => AvType.Professional,
            "auroravisionstudio.exe" => AvType.Professional,
            "fabimagestudio.exe" => AvType.Professional,
            //
            "adaptivevisionexecutor.exe" => AvType.Runtime,
            "auroravisionexecutor.exe" => AvType.Runtime,
            "fabimageexecutor.exe" => AvType.Runtime,
            //
            "avl.dll" => AvType.Library,
            "fil.dll" => AvType.Library,
            //
            "deeplearningeditor.exe" => AvType.DeepLearning,
            _ => throw new InvalidAppTypeNameException(name)
        };
    }
    /// <summary>
    /// Find matching <see cref="ProductType"/> based on the filename.
    /// </summary>
    /// <returns>Matching <see cref="ProductType"/></returns>
    /// <inheritdoc cref="GetAvTypeFromFilename(string)(string)"/>
    public static ProductType FromFilepath(string filepath) => FromAvType(GetAvTypeFromFilename(filepath));
    public bool IsExecutable => Type != AvType.Library;
    public int CompareTo(object? obj) => CompareTo(obj as ProductType);

 
}