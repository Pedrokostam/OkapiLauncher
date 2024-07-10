using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.Models
{
    public partial class SimpleProcess : ObservableObject, IEquatable<SimpleProcess>
    {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _mainWindowTitle;
        public SimpleProcess(Process proc)
        {
            Id = proc.Id;
            MainWindowTitle = proc.MainWindowTitle;
            proc.Dispose();
        }

        public bool Equals(SimpleProcess? other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Id == other.Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SimpleProcess);
        }
        [RelayCommand]
        private void Kill()
        {
            using Process p = Process.GetProcessById(Id);
            p.Kill();
        }
    }
}
