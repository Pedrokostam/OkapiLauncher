using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher.Models.Messages;
public sealed record KillProcessRequest(SimpleProcess Process,object ViewModel)
{
}
