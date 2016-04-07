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
using Svg.Transforms;

namespace LocalPefChartinator
{
    public class Program
    {
        private static readonly SvgColourServer LineColor = new SvgColourServer(Color.Blue);
        private const int DayWidth = 50;
        private const int SideColumnWidth = 60;
        private const int DaysPerWeek = 7;
        private const int Weeks = 3;
        private const int TopRow = 70;
        private const int MaxPef = 710;
        private const float PefSize = 1f;
        private const int DayRow = 230;
        private const int DateRow = 230;

        private const float ChartHeight = MaxPef * PefSize;
        private const float ChartWidth = Weeks*DaysPerWeek*DayWidth;

        private const float Width = ChartWidth + SideColumnWidth * 2;
        private const float Height = TopRow + ChartHeight + DayRow + DateRow;
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
                Height = Height + Padding * 2
            };
            var contentGroup = new SvgGroup()
            {
                Transforms = new SvgTransformCollection {new SvgTranslate(Padding, Padding)}
            };
            contentGroup.Children.Add(new SvgRectangle()
            {   
                Width = Width,
                Height =  Height,
                Fill =  new SvgColourServer(Color.Transparent),
                Stroke = LineColor
            });
            contentGroup.Children.Add(new SvgLine()
            {
                StartX = SideColumnWidth,
                StartY = 0,
                EndX = SideColumnWidth,
                EndY = Height,
                Stroke = LineColor
            });
            contentGroup.Children.Add(new SvgLine()
            {
                StartX = Width - SideColumnWidth,
                StartY = 0,
                EndX = Width - SideColumnWidth,
                EndY = Height,
                Stroke = LineColor
            });
            contentGroup.Children.Add(GetTopGroup());
            contentGroup.Children.Add(GetChartGroup());
            contentGroup.Children.Add(GetDayGroup());
            contentGroup.Children.Add(GetDateGroup());
            document.Children.Add(contentGroup);
            return document.GetXML();
        }

        private static SvgElement GetTopGroup()
        {
            SvgGroup group = new SvgGroup()
            {
                Transforms = new SvgTransformCollection() { new SvgTranslate(SideColumnWidth, 0)}
            };
            group.Children.Add(new SvgLine()
            {
                StartX = 0,
                StartY = TopRow,
                EndX = ChartWidth,
                EndY = TopRow,
                Stroke = LineColor
            });
            return group;
        }

        private static SvgElement GetChartGroup()
        {
            SvgGroup group = new SvgGroup()
            {
                Transforms = new SvgTransformCollection() {new SvgTranslate(SideColumnWidth, TopRow)}
            };
            group.Children.Add(new SvgLine()
            {
                StartX = 0,
                StartY = ChartHeight,
                EndX = ChartWidth,
                EndY = ChartHeight,
                Stroke = LineColor
            });
            return group;
        }

        private static SvgElement GetDayGroup()
        {
            SvgGroup group = new SvgGroup()
            {
                Transforms = new SvgTransformCollection() { new SvgTranslate(SideColumnWidth, TopRow + ChartHeight)}
            };
            group.Children.Add(new SvgLine()
            {
                StartX = 0,
                StartY = DayRow,
                EndX = ChartWidth,
                EndY = DayRow,
                Stroke = LineColor
            });
            return group;
        }

        private static SvgElement GetDateGroup()
        {
            SvgGroup group = new SvgGroup()
            {
                Transforms = new SvgTransformCollection() { new SvgTranslate(SideColumnWidth, TopRow + ChartHeight + DayRow) }
            };
            group.Children.Add(new SvgLine()
            {
                StartX = 0,
                StartY = DateRow,
                EndX = ChartWidth,
                EndY = DateRow,
                Stroke = LineColor
            });
            return group;
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
