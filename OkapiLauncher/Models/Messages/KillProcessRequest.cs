﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraVisionLauncher.Models.Messages;
public sealed record KillProcessRequest(SimpleProcess Process,object ViewModel)
{
}