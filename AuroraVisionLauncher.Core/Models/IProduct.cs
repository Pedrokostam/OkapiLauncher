namespace AuroraVisionLauncher.Core.Models;

public interface IProduct
{
    ProductBrand Brand { get; }
    ProductType Type { get; }
    IAvVersion Version { get; }
}

