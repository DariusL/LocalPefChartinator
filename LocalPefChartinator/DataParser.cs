using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace LocalPefChartinator
{
    public class DataParser
    {
        public static IReadOnlyList<DataPoint> Parse(IReadOnlyList<Tuple<string, string>> data, string timezone)
        {
            DateTimeZone tz = ParseTimeZone(timezone);
            return data.Select(item => Parse(item, tz)).ToArray();
        }

        private static DataPoint Parse(Tuple<string, string> tuple, DateTimeZone timezone)
        {
            long time = long.Parse(tuple.Item1);
            Instant instant = Instant.FromSecondsSinceUnixEpoch(time);
            int pef = int.Parse(tuple.Item2);
            return new DataPoint(pef, new ZonedDateTime(instant, timezone));
        }

        public static TzdbZoneLocation ParseLocale(string locale)
        {
            return locale.Contains("_") ? ParseLocation(locale.Substring(3, 2)) : null;
        }

        public static TzdbZoneLocation ParseLocation(string locationString)
        {
            TzdbDateTimeZoneSource timeZoneSource = TzdbDateTimeZoneSource.Default;
            return
                timeZoneSource.ZoneLocations.First(
                    location => location.CountryCode.Equals(locationString, StringComparison.OrdinalIgnoreCase));
        }

        public static ZonedDateTime ParseIsoTime(string isoTimeString)
        {
            return ZonedDateTime.FromDateTimeOffset(DateTimeOffset.Parse(isoTimeString, CultureInfo.InvariantCulture));
        }

        public static DateTimeZone ParseTimeZone(string zone)
        {
            return DateTimeZone.ForOffset(OffsetPattern.GeneralInvariantPatternWithZ.Parse(zone).Value);
        }
    }
}
