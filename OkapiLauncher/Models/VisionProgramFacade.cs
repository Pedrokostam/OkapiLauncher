using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Projects;
using AuroraVisionLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.Models;
public partial class VisionProjectFacade : ObservableObject, IVisionProject
{
    public string Name { get; }
    public AvVersionFacade Version { get; }
    public AvVersionFacade? VersionNonMissing => Version.IsUnknown ? null : Version;
    public string Path { get; }
    public ProductType Type { get; }
    public ProductBrand Brand { get; }
    public DateTime DateModified { get; }

    public bool Exists => File.Exists(Path);

    IAvVersion IProduct.Version => Version;

    public VisionProjectFacade(VisionProject visprog)
        : this(visprog.Name, visprog.Version, visprog.Path, visprog.Type, visprog.Brand, visprog.DateModified)
    {
    }
    public VisionProjectFacade(string name, IAvVersion version, string path, ProductType type, ProductBrand brand, DateTime dateModified)/*:this(new VisionProgram(name,new(version),path,type))*/
    {
        Name = name;
        Path = path;
        Brand = brand;
        Type = type;
        Version = new AvVersionFacade(version);
        DateModified = dateModified;
    }
    [RelayCommand]
    private void CopyPathToClipboard()
    {
        Clipboard.SetText(Path);
    }
    [RelayCommand]
    private void OpenProgramFolder() => ExplorerHelper.OpenExplorerAndSelect(Path);

    public override string ToString() => $"{Name}, {Type}, {Version}";
}
