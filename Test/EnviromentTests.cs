using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalPefChartinator.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class EnviromentTests
    {
        [TestMethod]
        public void TestEnviromentHasPdfKey()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Config.Get(Config.KeyHtml2Pdf)));
        }
    }
}
