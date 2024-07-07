using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AuroraVisionLauncher.Helpers;
public static class ResourceHelper
{
    /// <summary>
    /// Retrieves the stream of the given resource.
    /// </summary>
    /// <param name="pathToResource">Path to the resource starting from the root of the project. E.g. <code>Resources/Icons/AppIcon.ico</code></param>
    /// <returns></returns>
    public static Stream GetResourceStream(string pathToResource)
    {
        var stream = Application.GetResourceStream(new Uri(pathToResource, UriKind.Relative))?.Stream;
        return stream is null ? throw new FileNotFoundException(pathToResource) : stream;
    }
    /// <summary>
    /// Retrieves the icon resource at the given path.
    /// </summary>
    /// <inheritdoc cref="GetResourceStream(string)"/>
    public static Icon? GetIconResource(string pathToResource)
    {
        if (GetResourceStream(pathToResource) is not Stream stream)
        {
            throw new FileNotFoundException(pathToResource);
        }
        return new Icon(stream);
    }
}
