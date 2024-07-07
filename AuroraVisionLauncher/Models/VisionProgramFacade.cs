using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AuroraVisionLauncher.Core.Models.Programs;
using AuroraVisionLauncher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.Models;
public partial class VisionProgramFacade : ObservableObject, IVisionProgram
{
    private readonly VisionProgram _visionProgram;
    public string Name => _visionProgram.Name;
    public Version Version => _visionProgram.Version;
    public Version? NonMissingVersion => Version == VisionProgram.MissingVersion ? null : Version;
    public string Path => _visionProgram.Path;
    public ProgramType Type => _visionProgram.Type;

    public bool Exists => _visionProgram.Exists;

    public VisionProgramFacade(VisionProgram visprog)
    {
        _visionProgram = visprog;
    }
    public VisionProgramFacade(string name, Version version, string path, ProgramType type):this(new VisionProgram(name,version,path,type))
    {
            
    }
    [RelayCommand]
    private void CopyPathToClipboard()
    {
        Clipboard.SetText(Path);
    }
    [RelayCommand]
    private void OpenProgramFolder() => ExplorerHelper.OpenExplorer(Path);
}
