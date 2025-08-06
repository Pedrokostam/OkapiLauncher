﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher.Helpers;
internal static class ExplorerHelper
{
    /// <summary>
    /// Open file explorer one folder above the path and select the path.
    /// </summary>
    /// <param name="path"></param>
    public static void OpenExplorerAndSelect(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = @$"/select, ""{path}""",
        });
    }
    public static void OpenExplorer(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = @$"""{path}""",
        });
    }
}
