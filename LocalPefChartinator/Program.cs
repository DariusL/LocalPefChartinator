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
        private const float DataHeight = ChartHeight + DayRow + DateRow;

        private const float ChartHeight = MaxPef * PefSize;
        private const float ChartWidth = Weeks*DaysPerWeek*DayWidth;

        private const float Width = ChartWidth + SideColumnWidth * 2;
        private const float Height = TopRow + DataHeight;
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
            var contentGroup = GetContentGroup(Padding, Padding);
            document.Children.Add(contentGroup);
            return document.GetXML();
        }

        private static SvgGroup GetContentGroup(float left, float top)
        {
            SvgGroup contentGroup = Group(left, top);
            contentGroup.Children.Add(new SvgRectangle()
            {
                Width = Width,
                Height = Height,
                Fill = new SvgColourServer(Color.Transparent),
                Stroke = LineColor
            });
            contentGroup.Children.Add(GetBorderGroup(0, 0));
            contentGroup.Children.Add(GetDataGroup(SideColumnWidth, TopRow));
            return contentGroup;
        }

        private static SvgGroup GetTopGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
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

        private static SvgGroup GetDataGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
            group.Children.Add(GetChartGroup(0, 0));
            group.Children.Add(GetDayGroup(0, ChartHeight));
            group.Children.Add(GetDateGroup(0, ChartHeight + DayRow));

            for (int i = 0; i <= Weeks * DaysPerWeek * SegmentsPerDay; i++)
            {
                float x = i * SegmentWidth;
                bool endOfWeek = (i % (SegmentsPerDay * DaysPerWeek)) == 0;
                bool endOfDay = (i % SegmentsPerDay) == 0;
                group.Children.Add(Line(x, 0, x, endOfDay ? DataHeight : ChartHeight, endOfWeek ? 2.5f : 1f));
            }

            group.Children.Add(Line(0, ChartHeight + DayRow, ChartWidth, ChartHeight + DayRow));

            return group;
        }

        private static SvgGroup GetBorderGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
            group.Children.Add(GetTopGroup(SideColumnWidth, 0));
            group.Children.Add(GetColumnGroup(0, TopRow));
            group.Children.Add(GetColumnGroup(ChartWidth + SideColumnWidth, TopRow));
            return group;
        }

        private static SvgGroup GetChartGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
            for (int i = 0; i < Weeks * DaysPerWeek; i += 2)
            {
                float x = DayWidth * i;
                group.Children.Add(Rect(x, 0, DayWidth, ChartHeight, Color.Cyan));
            }
            for (int i = 0; i <= MaxPef; i+=PefIncrement)
            {
                int pef = MaxPef - i;
                group.Children.Add(Line(0, i, ChartWidth, i, pef % 100 == 0 ? 2f : 1f));
            }
            return group;
        }

        private static SvgGroup GetDayGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
            return group;
        }

        private static SvgGroup GetDateGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
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

        private static SvgGroup GetColumnGroup(float left, float top)
        {
            SvgGroup group = Group(left, top);
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

            group.Children.Add(Text(SideColumnWidth / 2f, ChartHeight + DayRow / 2f, "Day"));
            group.Children.Add(Line(0, ChartHeight + DayRow, SideColumnWidth, ChartHeight + DayRow));
            group.Children.Add(Text(SideColumnWidth / 2f, ChartHeight + DayRow + DateRow / 2f, "Date"));

            return group;
        }

        private static SvgElement Line(float x1, float y1, float x2, float y2)
        {
            return Line(x1, y1, x2, y2, 1f);
        }


        private static SvgLine Line(float x1, float y1, float x2, float y2, float width)
        {
            return new SvgLine()
            {
                StartX = x1,
                StartY = y1,
                EndX = x2,
                EndY = y2,
                Stroke = LineColor,
                StrokeWidth = new SvgUnit(width)
            };
        }

        private static SvgRectangle Rect(float x, float y, float width, float height)
        {
            return Rect(x, y, width, height, Color.White);
        }

        private static SvgRectangle Rect(float x, float y, float width, float height, Color color)
        {
            return new SvgRectangle()
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Stroke = LineColor,
                Fill = new SvgColourServer(color)
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

        private static SvgGroup Group(float left, float top)
        {
            return new SvgGroup()
            {
                Transforms = Translate(left, top)
            };
        }

        private static SvgTransformCollection Translate(float left, float top)
        {
            return new SvgTransformCollection(){new SvgTranslate(left, top)};
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
