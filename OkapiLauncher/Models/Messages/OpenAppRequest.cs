using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Core.Models.Apps;

namespace OkapiLauncher.Models.Messages;
public sealed record OpenAppRequest
{
    public OpenAppRequest(IAvApp app, IEnumerable<string> arguments)
    {
        App = app;
        Arguments = arguments.ToArray();
    }
    public OpenAppRequest(IAvApp app) : this(app, [])
    {
        
    }
    public IAvApp App { get; }
    public string[] Arguments { get; }
}
