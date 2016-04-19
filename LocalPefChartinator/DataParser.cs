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
        public static IReadOnlyList<DataPoint> Parse(IEnumerable<SerializedDataPoint> data, DateTimeZone timezone)
        {
            try
            {
                return data.Select(item => Parse(item, timezone)).ToArray();
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse data", e);
            }
        }

        private static DataPoint Parse(SerializedDataPoint serialized, DateTimeZone timezone)
        {
            long time = long.Parse(serialized.Time);
            Instant instant = Instant.FromSecondsSinceUnixEpoch(time);
            int pef = int.Parse(serialized.Pef);
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

            try
            {
                return DateTimeZone.ForOffset(OffsetPattern.GeneralInvariantPatternWithZ.Parse(zone).Value);
            }
            catch (Exception e)
            {
                throw new ArgumentException(String.Format("Invalid zone {0}", zone), e);
            }
        }
    }
}
