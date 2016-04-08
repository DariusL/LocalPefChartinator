using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace LocalPefChartinator
{
    class DataParser
    {
        public static IReadOnlyList<DataPoint> Parse(IReadOnlyList<Tuple<string, string>> data, DateTimeZone timezone)
        {
            return data.Select(tuple => Parse(tuple, timezone)).ToArray();
        }

        private static DataPoint Parse(Tuple<string, string> tuple, DateTimeZone timezone)
        {
            long time = long.Parse(tuple.Item1);
            Instant instant = Instant.FromSecondsSinceUnixEpoch(time);
            int pef = int.Parse(tuple.Item2);
            return new DataPoint(pef, new ZonedDateTime(instant, timezone));
        }
    }
}
