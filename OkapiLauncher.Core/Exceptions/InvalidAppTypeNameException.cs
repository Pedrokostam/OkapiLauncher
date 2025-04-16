using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher.Core.Exceptions;
public class InvalidAppTypeNameException(string name) : Exception($"Given name does not match any app type: {name}")
{
    public string Name { get; } = name;
}
