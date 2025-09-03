using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OkapiLauncher.Controls.Utilities;

namespace OkapiLauncher.Tests.MSTest;
[TestClass]
public class ButtonSettingsTests
{
    [TestMethod]
    public void EnsureDefaultOrderHasAllBuyttons()
    {
        var orders = ButtonSettings.DefaultOrder.OrderBy(x => x);
        var allFlags = OkapiLauncher.Helpers.EnumExtensions.GetAllStandaloneFlags<VisibleButtons>().OrderBy(x => x).ToList();
        Assert.IsTrue(orders.SequenceEqual(allFlags));
    }
}
