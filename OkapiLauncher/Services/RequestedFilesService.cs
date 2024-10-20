using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Contracts.Services;

namespace AuroraVisionLauncher.Services;
public class RequestedFilesService : IRequestedFilesService
{
    readonly private Queue<string> _files;
    public RequestedFilesService()
    {
        var args = Environment.GetCommandLineArgs();
        _files = new Queue<string>();
        foreach (var arg in args)
        {
            _files.Enqueue(arg);
        }

    }
    public string? GetNextRequestedFile()
    {
        if (_files.Count > 0)
        {
            return _files.Dequeue();
        }
        return null;
    }
}
