using System.Diagnostics;
using System.Text.RegularExpressions;
using AuroraVisionLauncher.Core.Models.Projects;

namespace AuroraVisionLauncher.Core.Models;
public enum AvBrand
{
    Aurora,
    Adaptive,
    FabImage,
}
public class ProductBrand:IComparable<ProductBrand>
{
    public static readonly ProductBrand Aurora = new ProductBrand("Aurora Vision", AvBrand.Aurora);
    public static readonly ProductBrand Adaptive = new ProductBrand("Adaptive Vision", AvBrand.Adaptive);
    public static readonly ProductBrand FabImage = new ProductBrand("FabImage", AvBrand.FabImage);

    private readonly static Regex _brandFinder;
    private readonly List<ProductBrand> _supportedBrands = [];

    internal static ProductBrand FromAvBrand(AvBrand type)
    {
        return type switch
        {
            AvBrand.Aurora => Aurora,
            AvBrand.Adaptive => Adaptive,
            AvBrand.FabImage => FabImage,
            _ => throw new NotSupportedException(),
        };
    }

    static ProductBrand()
    {
        Aurora._supportedBrands.Add(Aurora);
        Aurora._supportedBrands.Add(Adaptive);

        Adaptive._supportedBrands.Add(Adaptive);

        FabImage._supportedBrands.Add(FabImage);

        string brandFinderPattern = $"({Aurora.Name}|{FabImage.Name}|{Adaptive.Name})";
        _brandFinder = new Regex(brandFinderPattern,
                                RegexOptions.Compiled,
                                TimeSpan.FromMilliseconds(200));
    }

    public string Name { get; }
    public AvBrand Brand { get; }
    internal string NameNoSpace { get; }
    public IReadOnlyCollection<ProductBrand> SupportedBrands => _supportedBrands.AsReadOnly();

    private ProductBrand(string name, AvBrand type)
    {
        Name = name;
        Brand = type;
        NameNoSpace = name.Replace(" ", "");

    }
    /// <summary>
    /// Tries to find the License.txt in the given folder and find what brand it is referring to.
    /// </summary>
    /// <param name="rootFolder">Root folder of the app, where License.txt is.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static ProductBrand FindBrandByLicense(string rootFolder)
    {
        ArgumentNullException.ThrowIfNull(rootFolder);
        FileInfo? licenseFile = new FileInfo(Path.Combine(rootFolder, "License.txt"));
        if (licenseFile is null || !licenseFile.Exists)
        {
            throw new NotSupportedException("Cannot determine brand without the license file");
        }
        using var licenseStream = licenseFile.OpenText();
        var line = licenseStream.ReadLine();
        while (line is not null)
        {
            var match = _brandFinder.Match(line);
            if (!match.Success)
            {
                line = licenseStream.ReadLine();
                continue;
            }
            return GetBrandByName(match.Value);
        }
        throw new NotSupportedException("Cannot determine brand from the license file");
    }
    public static ProductBrand GetBrandByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var comp = StringComparison.OrdinalIgnoreCase;
        if (string.Equals(name, Aurora.Name, comp))
            return Aurora;
        if (string.Equals(name, FabImage.Name, comp))
            return FabImage;
        if (string.Equals(name, Adaptive.Name, comp))
            return Adaptive;
        throw new ArgumentException($"{name} does not match any brand.", nameof(name));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="signature">The name of the root element in .__proj file.</param>
    /// <returns></returns>
    public static ProductBrand GetBrandByProjSignature(string signature)
    {
        ArgumentNullException.ThrowIfNull(signature);
        var comp = StringComparison.OrdinalIgnoreCase;
        if (signature.StartsWith(Aurora.Name, comp))
            return Aurora;
        if (signature.StartsWith(FabImage.Name, comp))
            return FabImage;
        if (signature.StartsWith(Adaptive.Name, comp))
            return Adaptive;
        throw new ArgumentException($"{signature} does not match any brand signature.", nameof(signature));
    }


    public bool SupportsBrand(ProductBrand brand)
    {
        return _supportedBrands.Contains(brand);
    }

    public int CompareTo(ProductBrand? other)
    {
        if(other == null) return 1;
        return Brand.CompareTo(other.Brand);
    }
    public override string ToString() => Name;
}
