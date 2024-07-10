using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Programs;
using AuroraVisionLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.Models;
public partial class VisionProgramFacade : ObservableObject, IVisionProgram
{
    public string Name { get; }
    public AvVersionFacade Version { get; }
    public AvVersionFacade? VersionNonMissing => Version.IsUnknown ? null : Version;
    public string Path { get; }
    public ProgramType Type { get;  }

    public bool Exists => File.Exists(Path);

    IAvVersion IVisionProgram.Version => Version;

    public VisionProgramFacade(VisionProgram visprog):this(visprog.Name,visprog.Version,visprog.Path,visprog.Type)
    {
    }
    public VisionProgramFacade(string name, IAvVersion version, string path, ProgramType type)/*:this(new VisionProgram(name,new(version),path,type))*/
    {
        Name = name;
        Path = path;
        Type = type;
        Version = new AvVersionFacade(version);
    }
    [RelayCommand]
    private void CopyPathToClipboard()
    {
        Clipboard.SetText(Path);
    }
    [RelayCommand]
    private void OpenProgramFolder() => ExplorerHelper.OpenExplorer(Path);
}
