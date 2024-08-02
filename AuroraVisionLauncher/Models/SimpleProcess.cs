using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AuroraVisionLauncher.Models.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AuroraVisionLauncher.Models
{
    public partial class SimpleProcess : ObservableObject, IEquatable<SimpleProcess>, IComparable<SimpleProcess>
    {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TrimmedTitle))]
        private string _mainWindowTitle;
        [ObservableProperty]
        private string _processName;
        [ObservableProperty]
        private DateTime _startTime;
        private readonly IMessenger _messenger;
        public string Path { get; }


        public SimpleProcess(Process proc, IMessenger messenger) : this(proc, messenger, proc.MainModule!.FileName ?? throw new ArgumentNullException("path"))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="messenger"></param>
        /// <param name="path">Path to the process' executable. Geting that path from process is possible, but might be slow.</param>
        public SimpleProcess(Process proc, IMessenger messenger, string path)
        {
            ProcessName = proc.ProcessName;
            Id = proc.Id;
            MainWindowTitle = proc.MainWindowTitle;
            StartTime = proc.StartTime;
            _messenger = messenger;
            Path = path;
        }

        public SimpleProcess(SimpleProcess other)
        {
            ProcessName = other.ProcessName;
            Id = other.Id;
            MainWindowTitle = other.MainWindowTitle;
            StartTime = other.StartTime;
            _messenger = other._messenger;
            Path = other.Path;
        }


        public string TrimmedTitle
        {
            get
            {
                var dash = MainWindowTitle.IndexOf('-', StringComparison.Ordinal);
                if (dash >= 0)
                {
                    return MainWindowTitle[(dash + 1)..].Trim();
                }
                return MainWindowTitle;
            }
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
            _messenger.Send(new KillProcessRequest(this));
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
            using var proc = Process.GetProcessById(Id);
            var handle = proc.MainWindowHandle;
            var placement = new WINDOWPLACEMENT();
            GetWindowPlacement(handle, ref placement);
            if (placement.showCmd == 2)
            {
                placement.showCmd = 1;
                SetWindowPlacement(handle, ref placement);
            }
            SetForegroundWindow(handle);
        }

        public int CompareTo(SimpleProcess? other)
        {
            if (other is null)
            {
                return 1;
            }
            return MainWindowTitle.CompareTo(other.MainWindowTitle);
        }

        public SimpleProcess Clone()
        {
            return new SimpleProcess(this);
        }

        public void UpdateFrom(SimpleProcess donor)
        {
            ArgumentNullException.ThrowIfNull(donor);
            if (Id != donor.Id)
            {
                return;
            }
            MainWindowTitle = donor.MainWindowTitle;
            
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
