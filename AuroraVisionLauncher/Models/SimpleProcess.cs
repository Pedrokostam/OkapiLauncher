using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        [ObservableProperty]
        private DateTime _startTime;
        private readonly IntPtr _windowHandle;
        public SimpleProcess(Process proc)
        {
            ProcessName = proc.ProcessName;
            Id = proc.Id;
            MainWindowTitle = proc.MainWindowTitle;
            StartTime = proc.StartTime;
            _windowHandle = proc.MainWindowHandle;
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
        private void KillAsk()
        {
            var res = MessageBox.Show($"Are you sure you want to kill this process:\n{ProcessName} - {MainWindowTitle}?",
                                      "Confirm processing ending",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Warning);
            if (res == MessageBoxResult.No)
            {
                return;
            }
            Kill();
        }

        [RelayCommand]
        private void Kill()
        {
            using Process p = Process.GetProcessById(Id);
            p.Kill();
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [RelayCommand]
        private void BringToFocus()
        {
            var placement = new WINDOWPLACEMENT();
            GetWindowPlacement(_windowHandle, ref placement);
            if(placement.showCmd == 2)
            {
                placement.showCmd = 1;
                SetWindowPlacement(_windowHandle, ref placement);
            }
            SetForegroundWindow(_windowHandle);
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
