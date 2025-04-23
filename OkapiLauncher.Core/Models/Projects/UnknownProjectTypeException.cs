using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Search;

namespace OkapiLauncher.Core.Models.Projects;
public class UnknownProjectTypeException : Exception
{
    public UnknownProjectTypeException(string path, string name) : base("Could not determine project type.")
    {
        Path = path;
        Name = name;
    }

    public string Path { get; }
    public string Name { get; }

}
