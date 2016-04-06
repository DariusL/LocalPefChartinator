using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Svg;

namespace LocalPefChartinator
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.File.WriteAllText("out.svg", generate(null));
        }

        static string generate(IEnumerable<DataPoint> data)
        {
            var document = new SvgDocument()
            {
                Width = 100,
                Height = 100
            };
            document.Children.Add(new SvgCircle() { CenterX = 50, CenterY = 50, Radius = 40, str });
            
        }
    }

    struct DataPoint
    {
        public readonly int pef;
        public readonly Instant time;

        public DataPoint(int pef, Instant time)
        {
            this.pef = pef;
            this.time = time;
        }
    }
}
