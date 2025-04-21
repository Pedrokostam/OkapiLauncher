using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using OkapiLauncher.Core.Exceptions;
using OkapiLauncher.Core.Models.Projects;

namespace OkapiLauncher.Core.Models;
public enum AvBrand
{
    Any,
    Aurora,
    Adaptive,
    FabImage,
}
public class ProductBrand : IComparable<ProductBrand>
{
    public static readonly ProductBrand Aurora = new ProductBrand("Aurora Vision", AvBrand.Aurora);
    public static readonly ProductBrand Adaptive = new ProductBrand("Adaptive Vision", AvBrand.Adaptive);
    public static readonly ProductBrand FabImage = new ProductBrand("FabImage", AvBrand.FabImage);
    public static readonly ProductBrand AnyBrand = new ProductBrand("Indeterminate brand", AvBrand.Any);

    private readonly static Regex _brandFinder;
    private readonly List<ProductBrand> _supportedBrands = [];

    internal static ProductBrand FromAvBrand(AvBrand type)
    {
        return type switch
        {
            AvBrand.Any => AnyBrand,
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
        Aurora._supportedBrands.Add(AnyBrand);

        Adaptive._supportedBrands.Add(Adaptive);
        Adaptive._supportedBrands.Add(AnyBrand);

        FabImage._supportedBrands.Add(FabImage);
        FabImage._supportedBrands.Add(AnyBrand);

        AnyBrand._supportedBrands.Add(Aurora);
        AnyBrand._supportedBrands.Add(FabImage);
        AnyBrand._supportedBrands.Add(Adaptive);
        AnyBrand._supportedBrands.Add(AnyBrand);

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
    /// <exception cref="UndeterminableBrandException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static ProductBrand? FindBrandByLicense(string rootFolder)
    {
        ArgumentNullException.ThrowIfNull(rootFolder);
        FileInfo? licenseFile = new FileInfo(Path.Combine(rootFolder, "License.txt"));
        if (licenseFile is null || !licenseFile.Exists)
        {
            return null;
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
        throw UndeterminableBrandException.ForLicense(licenseFile.FullName, null);
    }
    /// <exception cref="UndeterminableBrandException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static ProductBrand? FindBrandFromHeaderFile(string rootFolder)
    {
        ArgumentNullException.ThrowIfNull(rootFolder);
        // STD.h does not change name between brands and all header files should contain the brand name anyway
        FileInfo? headerFile = new FileInfo(Path.Combine(rootFolder, "include", "STD.h"));
        if (headerFile is null || !headerFile.Exists)
        {
            return null;
        }
        using var headerStream = headerFile.OpenText();
        var line = headerStream.ReadLine();
        while (line is not null)
        {
            if (!line.Contains("Library", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var match = _brandFinder.Match(line);
            if (match.Success)
            {
                return GetBrandByName(match.Value);
            }
        }
        throw UndeterminableBrandException.ForHeader(headerFile.FullName);
    }
    /// <exception cref="UndeterminableBrandException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
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
        throw new InvalidBrandNameException(name);
    }
    /// <exception cref="ArgumentNullException"></exception>
    public static ProductBrand? GetBrandByExeName(string filepath)
    {
        string? name = Path.GetFileName(filepath);
        ArgumentNullException.ThrowIfNull(name);
        var comp = StringComparison.OrdinalIgnoreCase;
        if (name.Contains(Aurora.NameNoSpace, comp))
            return Aurora;
        if (name.Contains(FabImage.NameNoSpace, comp))
            return FabImage;
        if (name.Contains(Adaptive.NameNoSpace, comp))
            return Adaptive;
        return null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filepath">Filepath to the executable.</param>
    /// <param name="type">Optional <see cref="ProductType"/>. If present, will not determione type from filepath.</param>
    /// <returns>Matched <see cref="ProductBrand"/></returns>
    /// <exception cref="UndeterminableBrandException"></exception>
    public static ProductBrand FromFilepath(string filepath, ProductType? type = null)
    {
        type ??= ProductType.FromFilepath(filepath);
        string root = type.GetRootFolder(filepath);
        var brand = ProductBrand.GetBrandByExeName(filepath);
        if (brand is not null)
        {
            return brand;
        }
        brand = FindBrandByLicense(root);
        if (brand is not null)
        {
            return brand;
        }
        brand = FindBrandFromHeaderFile(root);
        if (brand is null)
        {
            throw UndeterminableBrandException.ForAllApplicable(filepath, type.Type);
        }
        return brand;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="signature">The name of the root element in .__proj file.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UndeterminableBrandException"></exception>
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
        throw new InvalidBrandNameException(signature);
    }

    public string GetLicenseKeyFolderPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Join(localAppData, Name, "Licenses");
    }

    public bool SupportsBrand(ProductBrand brand)
    {
        return _supportedBrands.Contains(brand);
    }

    public int CompareTo(ProductBrand? other)
    {
        if (other == null)
            return 1;
        return Brand.CompareTo(other.Brand);
    }
    public override string ToString() => Name;
    public static implicit operator AvBrand(ProductBrand brand) => brand.Brand;
}
