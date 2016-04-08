using NodaTime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Svg;
using Svg.Transforms;

namespace LocalPefChartinator
{
    public class Program
    {
       

        public static void Main(string[] args)
        {
            var now = new ZonedDateTime(SystemClock.Instance.Now, DateTimeZone.Utc);
            System.IO.File.WriteAllText("out.svg", ChartGenerator.Generate(
            new[]
            {
                new DataPoint(500, now), 
                new DataPoint(550, now.Plus(Duration.FromStandardDays(2).Plus(Duration.FromHours(5)))),
                new DataPoint(580, now.Plus(Duration.FromStandardDays(2).Plus(Duration.FromHours(8))))
            }
            ));
        }
    }

    internal struct DataPoint
    {
        public readonly int Pef;
        public readonly ZonedDateTime Time;

        public DataPoint(int pef, ZonedDateTime time)
        {
            Pef = pef;
            Time = time;
        }
    }
}
