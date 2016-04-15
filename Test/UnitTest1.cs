using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Text;
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

            var actual = Program.Group(data).Select(list => list.ToArray()).ToArray();

            AssertArraysEqual(expected, actual);
        }

        public static string ArrayToString(dynamic a)
        {
            if (a.GetType().IsArray)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[");
                string delim = "";
                foreach (var o in a)
                {
                    builder.Append(delim).Append(ArrayToString(o));
                    delim = ", ";
                }
                builder.Append("]");
                return builder.ToString();
            }
            else
            {
                return a.ToString();
            }
        }

        public static void AssertArraysEqual(dynamic expected, dynamic actual)
        {
            Assert.IsTrue(ArraysEqual(expected, actual), "Expected: <{0}>, actual: <{1}>", ArrayToString(expected), ArrayToString(actual));
        }

        public static bool ArraysEqual(dynamic a, dynamic b)
        {
            // Same objects or both null
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // Just one object is null
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            if (a.GetType().IsArray && b.GetType().IsArray)
            {
                if (a.Length == b.Length)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (!ArraysEqual(a[i], b[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            // fall back to simple value compare
            return Equals(a, b);
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
