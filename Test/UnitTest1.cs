using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LocalPefChartinator;
using LocalPefChartinator.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using NodaTime.TimeZones;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        private ZonedDateTime now = new ZonedDateTime(SystemClock.Instance.Now, DateTimeZone.Utc);

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

        [TestMethod]
        public void TestInterval()
        {
            Assert.AreEqual(0, ChartGenerator.PagesBetween(Point(now), Point(now.PlusDays(20))));
            Assert.AreEqual(1, ChartGenerator.PagesBetween(Point(now), Point(now.PlusDays(21))));
        }

        [TestMethod]
        public void TestPaging()
        {
            var data = new[]
            {
                Point(now), Point(now.PlusDays(5)), Point(now.PlusDays(20)),
                Point(now.PlusDays(21)), Point(now.PlusDays(25)),
                Point(now.PlusDays(50))
            };

            var expected = new[]
            {
                new[] {Point(now), Point(now.PlusDays(5)), Point(now.PlusDays(20))},
                new[] {Point(now.PlusDays(21)), Point(now.PlusDays(25))},
                new[] {Point(now.PlusDays(50))}
            };

            var actual = Program.Group(data).ToArray();

            AssertEquals(expected, actual);
        }

        public static void AssertEquals(object left, object right)
        {
            if (!(left is IEnumerable && right is IEnumerable))
            {
                Assert.AreEqual(left, right);
            }

            var leftEnumerable = left as IEnumerable<object>;
            var rightEnumerable = right as IEnumerable<object>;

            leftEnumerable.Zip<object, object, object>(rightEnumerable, (o, o1) => { AssertEquals(o, o1); return null;}).ToArray();
        }

        [TestMethod]
        public void TestPagesBetween()
        {
            var data = new[]
            {
                Point(now), Point(now.PlusDays(5)), Point(now.PlusDays(20)),
                Point(now.PlusDays(21)), Point(now.PlusDays(25)),
                Point(now.PlusDays(50))
            };
            var expected = new int[]
            {
                0, 0, 0,
                1, 1,
                2
            };
            var actual = data.Select(point => ChartGenerator.PagesBetween(data[0], point)).ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestIsAfter()
        {
            LocalDate now = this.now.Date;
            LocalDate later = now.PlusDays(1);
            LocalDate evenLater = now.PlusDays(2);

            Assert.IsFalse(now.IsAfter(now));
            Assert.IsTrue(evenLater.IsAfter(later));
            Assert.IsFalse(later.IsAfter(evenLater));
        }

        private static DataPoint Point(ZonedDateTime time)
        {
            return new DataPoint(500, time);
        }

        private static DateTimeZone Zone(int hours, int minutes)
        {
            return DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(hours, minutes));
        }
    }
}
