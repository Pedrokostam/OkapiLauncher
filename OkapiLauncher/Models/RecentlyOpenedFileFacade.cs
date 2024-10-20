using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OkapiLauncher.Core.Models;

namespace OkapiLauncher.Models;
public readonly record struct RecentlyOpenedFileFacade(string FilePath, DateTime OpenedOn, int Index)
{
    public override int GetHashCode()
    {
        // Customize the hash code generation
        // Example using HashCode.Combine for better distribution
        return string.GetHashCode(FilePath, StringComparison.OrdinalIgnoreCase);
    }
    public RecentlyOpenedFileFacade(RecentlyOpenedFile rof, int index) : this(rof.FilePath, rof.OpenedOn, index)
    {

    }
    public bool Equals(RecentlyOpenedFileFacade other)
    {
        return string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase);
    }
}
