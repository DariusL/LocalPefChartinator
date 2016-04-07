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
        private const int SegmentWidth = 15;
        private const int SegmentsPerDay = 4;
        private const int DayWidth = SegmentWidth * SegmentsPerDay;
        private const int SideColumnWidth = 60;
        private const int DaysPerWeek = 7;
        private const int Weeks = 3;
        private const int TopRow = 70;
        private const int MaxPef = 710;
        private const float PefSize = 1f;
        private const int DayRow = 230;
        private const int DateRow = 230;
        private const int PefIncrement = 10;

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
            contentGroup.Children.Add(Line(SideColumnWidth, 0, SideColumnWidth, Height));
            contentGroup.Children.Add(Line(Width - SideColumnWidth, 0, Width - SideColumnWidth, Height));
            contentGroup.Children.Add(GetTopGroup());
            contentGroup.Children.Add(GetChartGroup());
            contentGroup.Children.Add(GetDayGroup());
            contentGroup.Children.Add(GetDateGroup());
            contentGroup.Children.Add(CreateColumnGroup(0));
            contentGroup.Children.Add(CreateColumnGroup(ChartWidth + SideColumnWidth));
            document.Children.Add(contentGroup);
            return document.GetXML();
        }

        private static SvgGroup GetTopGroup()
        {
            SvgGroup group = new SvgGroup()
            {
                Transforms = new SvgTransformCollection() { new SvgTranslate(SideColumnWidth, 0)}
            };
            group.Children.Add(Line(0, TopRow, ChartWidth, TopRow));
            group.Children.Add(Line(0, 0, 0, TopRow));
            for (int i = 0; i < DaysPerWeek * Weeks * SegmentsPerDay + 1; i++)
            {
                int length = i % SegmentsPerDay == 0 ? TopRow : TopRow / 6;
                int x = i*SegmentWidth;
                group.Children.Add(Line(x, TopRow, x, TopRow - length));
            }
            return group;
        }

        private static SvgGroup GetChartGroup()
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

        private static SvgGroup GetDayGroup()
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

        private static SvgGroup GetDateGroup()
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

        private static SvgGroup CreateColumnGroup(float offset)
        {
            SvgGroup group = new SvgGroup()
            {
                Transforms = new SvgTransformCollection() {new SvgTranslate(offset, TopRow)}
            };
            for (int i = 0; i < SegmentsPerDay + 1; i++)
            {
                int x = i*SegmentWidth;
                group.Children.Add(Line(x, 0, x, ChartHeight));
            }

            for (int i = 0; i <= MaxPef; i += PefIncrement)
            {
                int pos = MaxPef - i;
                if (i%50 == 0 && i != 0)
                {
                    group.Children.Add(Rect(0, pos - PefIncrement, SideColumnWidth, PefIncrement*2));
                    group.Children.Add(Text(SideColumnWidth / 2f, pos + 6, i.ToString()));
                }
                else
                {
                    group.Children.Add(Line(0, pos, SideColumnWidth, pos));
                }
            }

            return group;
        }

        private static SvgLine Line(float x1, float y1, float x2, float y2)
        {
            return new SvgLine()
            {
                StartX = x1,
                StartY = y1,
                EndX = x2,
                EndY = y2,
                Stroke = LineColor
            };
        }

        private static SvgRectangle Rect(float x, float y, float width, float height)
        {
            return new SvgRectangle()
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Stroke = LineColor,
                Fill = new SvgColourServer(Color.White)
            };
        }

        private static SvgText Text(float x, float y, string text)
        {
            var svgText = new SvgText()
            {
                TextAnchor = SvgTextAnchor.Middle,
                Fill = new SvgColourServer(Color.Black),
                X = new SvgUnitCollection(){x},
                Y = new SvgUnitCollection(){y},
                FontSize = new SvgUnit(16)
            };
            svgText.Nodes.Add(new SvgContentNode(){Content = text});
            return svgText;
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
