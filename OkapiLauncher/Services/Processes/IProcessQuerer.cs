using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Core.Models.Apps;
using OkapiLauncher.Models;

namespace OkapiLauncher.Services.Processes;
public sealed class ProcessException : Exception
{

}
public interface IProcessQuerer
{
    SimpleProcess GetSingleAppStatus(IAvApp app );
    ImmutableArray<SimpleProcess> GetAppStatus(IAvApp app);
}
public class ProcessQuerer
{
    
}
public sealed class DiagnosticQuerer : IProcessQuerer
{
    public ImmutableArray<SimpleProcess> GetAppStatus(IAvApp app)
    {
        throw new NotImplementedException();
    }

    public SimpleProcess GetSingleAppStatus(IAvApp app)
    {
        throw new NotImplementedException();
    }
}
public sealed class WmiQuerer : IProcessQuerer
{
    public ImmutableArray<SimpleProcess> GetAppStatus(IAvApp app)
    {
        throw new NotImplementedException();
    }

    public SimpleProcess GetSingleAppStatus(IAvApp app)
    {
        throw new NotImplementedException();
    }
}