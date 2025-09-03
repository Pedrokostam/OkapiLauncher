using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OkapiLauncher.Controls.Utilities;

namespace OkapiLauncher.Models;
public partial class CheckedButton : ObservableObject
{
    [ObservableProperty]
    private bool _enabled;
    public VisibleButtons Button { get; }
    public CheckedButton(VisibleButtons button, bool enabled)
    {
        Button = button;
        Enabled = enabled;
    }
}
