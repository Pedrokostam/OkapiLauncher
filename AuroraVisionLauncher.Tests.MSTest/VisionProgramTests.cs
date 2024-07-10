using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Core.Models;
using AuroraVisionLauncher.Core.Models.Programs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuroraVisionLauncher.Tests.MSTest;
[TestClass]
public class VisionProgramTests
{
    public VisionProgramTests()
    {

    }

    [TestMethod]
    public void IsStudioOrRuntimeTest()
    {
        var collection = Enum.GetValues<ProgramType>();
        foreach (var item in collection)
        {
            if (item == ProgramType.None)
            {
                continue;
            }
            var prog = new VisionProgram("", AvVersion.MissingVersion, "", item);
            var s = prog.IsStudio();
            var r = prog.IsRuntime();
            Assert.IsTrue(s || r);
            Assert.IsFalse(s && r);
        }
    }
}
