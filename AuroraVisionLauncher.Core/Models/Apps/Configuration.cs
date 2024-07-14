using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace AuroraVisionLauncher.Core.Models.Apps;
internal record Configuration
{
    public Configuration(ProductType appType,  ProductBrand brand)
    {
        AppType = appType;
        Brand = brand;
    }
    public ProductType AppType { get; }
    public ProductBrand Brand { get; }
}
