using NodaTime;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Svg;

namespace LocalPefChartinator
{
    public class Program
    {
        private const int Width = 800;
        private const int Height = 700;
        private const int Padding = 10;

        public static void Main(string[] args)
        {
            System.IO.File.WriteAllText("out.svg", Generate());
        }

        private static string Generate()
        {
            var document = new SvgDocument
            {
                Width = Width + Padding * 2,
                Height = Height + Padding * 2,
                ViewBox = new SvgViewBox(-Padding, -Padding, Width, Height)
            };
            document.Children.Add(
                new SvgRectangle()
                {   
                    Width = Width,
                    Height =  Height,
                    Fill =  new SvgColourServer(Color.Blue),
                    Stroke = new SvgColourServer(Color.Aqua),
                    StrokeWidth = new SvgUnit(2)
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
