using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Core.Models.Apps;
using AuroraVisionLauncher.Core.Models.Programs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.Models
{
    public partial class ExecutableFacade : ObservableObject, IExecutable
    {
        readonly private Executable _executable;

        public string Name => _executable.Name;
        public string ExePath => _executable.ExePath;
        public Version Version => _executable.Version;
        public bool IsDevelopmentBuild => _executable.IsDevelopmentBuild;

        public ExecutableType ExecutableType => _executable.ExecutableType;

        [ObservableProperty]
        private string _compatibility = "";


        [ObservableProperty]
        private bool _isLaunched = false;

        public ExecutableFacade(Executable executable)
        {
            _executable = executable;
        }

        public bool CheckIfProcessIsRunning() => _executable.CheckIfProcessIsRunning();

        public bool SupportsProgram(ProgramInformation information) => _executable.SupportsProgram(information);

        [RelayCommand]
        private void OpenContainingFolder()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = @$"/select, ""{_executable.ExePath}"""
            });
        }
        [RelayCommand]
        private void LaunchWithoutProgram()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = _executable.ExePath
            });
        }
    }
}
