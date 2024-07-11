using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraVisionLauncher.Models
{
    public partial class SimpleProcess : ObservableObject, IEquatable<SimpleProcess>, IComparable<SimpleProcess>
    {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _mainWindowTitle;
        [ObservableProperty]
        private string _processName;
        public SimpleProcess(Process proc)
        {
            ProcessName = proc.ProcessName;
            Id = proc.Id;
            MainWindowTitle = proc.MainWindowTitle;
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
        private void Kill(bool ask = true)
        {
            if (ask)
            {
                var res = MessageBox.Show($"Are you sure you want to kill this process:\n{ProcessName} - {MainWindowTitle}?",
                                          "Confirm processing ending",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);
                if (res == MessageBoxResult.No)
                {
                    return;
                }
            }
            using Process p = Process.GetProcessById(Id);
            p.Kill();
        }

        public int CompareTo(SimpleProcess? other)
        {
            if (other is null)
            {
                return 1;
            }
            return MainWindowTitle.CompareTo(other.MainWindowTitle);
        }

        public static bool operator ==(SimpleProcess left, SimpleProcess right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(SimpleProcess left, SimpleProcess right)
        {
            return !(left == right);
        }

        public static bool operator <(SimpleProcess left, SimpleProcess right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(SimpleProcess left, SimpleProcess right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(SimpleProcess left, SimpleProcess right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(SimpleProcess left, SimpleProcess right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
