using System;
using System.Linq;
using LocalPefChartinator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using NodaTime.TimeZones;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestFindLithuaniaFromLocation()
        {
            var lithuania = DataParser.ParseLocation("LT");
            Assert.AreEqual("Lithuania", lithuania.CountryName);
        }

        [TestMethod]
        public void TestFindLithuaniaFromLocale()
        {
            var lithuania = DataParser.ParseLocale("lt_LT");
            Assert.AreEqual("Lithuania", lithuania.CountryName);
        }

        [TestMethod]
        public void TestParseTime()
        {
            DataParser.ParseIsoTime("2008-09-15T15:53:00+05:00");
        }

        [TestMethod]
        public void TestParseTimeZone()
        {
            Action<int, int, string> test = (hours, minutes, str) =>
            {
                Assert.AreEqual(Zone(hours, minutes), DataParser.ParseTimeZone(str));
            };

            test(0, 0, "Z");
            test(1, 0, "+01:00");
            test(-1, 0, "-01:00");
            test(1, 0, "+01");
            test(1, 0, "+01:00");
            test(-2, -30, "-02:30");
        }

        private static DateTimeZone Zone(int hours, int minutes)
        {
            return DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(hours, minutes));
        }
    }
}
