using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Contracts.Services;
public interface IRequestedFilesService
{
    string? GetNextRequestedFile();
}
