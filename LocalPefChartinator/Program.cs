using NodaTime;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Svg;

namespace LocalPefChartinator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.IO.File.WriteAllText("out.svg", Generate());
        }

        private static string Generate()
        {
            var document = new SvgDocument()
            {
                Width = 100,
                Height = 100
            };
            document.Children.Add(
                new SvgCircle() {
                    CenterX = 50, 
                    CenterY = 50, 
                    Radius = 40, 
                    Stroke = new SvgColourServer(Color.Green){StrokeWidth = new SvgUnit(4)},
                    Fill =  new SvgColourServer(Color.Yellow)
                });
            return document.GetXML();
        }
    }

    internal struct DataPoint
    {
        public readonly int Pef;
        public readonly Instant Time;

        public DataPoint(int pef, Instant time)
        {
            Pef = pef;
            Time = time;
        }
    }
}
