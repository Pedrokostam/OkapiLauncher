using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Core.Exceptions;
public class InvalidAppTypeNameException:Exception
{
    public InvalidAppTypeNameException(string name) : base($"Given name does not match any app type: {name}")
    {
        Name = name;
    }

    public string Name { get; }
}
