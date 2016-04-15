using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using Svg;
using Svg.Transforms;

namespace LocalPefChartinator
{
    public class ChartGenerator
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
        public const int TotalDays = Weeks * DaysPerWeek;

        private const float ChartHeight = MaxPef * PefSize;
        private const float ChartWidth = TotalDays * DayWidth;

        private const float Width = ChartWidth + SideColumnWidth * 2;
        private const float Height = TopRow + DataHeight;
        private const int Padding = 10;

        public static string Generate(IReadOnlyList<DataPoint> data)
        {
            var document = new SvgDocument
            {
                Width = Width + Padding * 2,
                Height = Height + Padding * 2
            };
            var contentGroup = GetContentGroup(Padding, Padding, data);
            document.Children.Add(contentGroup);
            return document.GetXML();
        }

        private static SvgGroup GetContentGroup(float left, float top, IReadOnlyList<DataPoint> data)
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
            contentGroup.Children.Add(GetDataGroup(SideColumnWidth, TopRow, data));
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
                int x = i * SegmentWidth;
                group.Children.Add(Line(x, TopRow, x, TopRow - length));

                bool morning = i % SegmentsPerDay - 1 == 0;
                bool evening = i % SegmentsPerDay - 3 == 0;

                float textY = TopRow * 3/5f;
                float textX = x + 4f;
                if(morning)
                    group.Children.Add(Text(textX, textY, -90, "6am"));
                if(evening)
                    group.Children.Add(Text(textX, textY, -90, "6pm"));
            }
            return group;
        }

        private static SvgGroup GetDataGroup(float left, float top, IReadOnlyList<DataPoint> data)
        {
            SvgGroup group = Group(left, top);
            var definitions = new SvgDefinitionList();
            var clip = new SvgClipPath()
            {
                ID = "data-clip"
            };
            clip.Children.Add(Rect(0, 0, ChartWidth, ChartHeight));
            definitions.Children.Add(clip);
            group.Children.Add(definitions);

            group.Children.Add(GetChartGroup(0, 0));
            group.Children.Add(GetDayGroup(0, ChartHeight));
            group.Children.Add(GetDateGroup(0, ChartHeight + DayRow));

            for (int i = 0; i <= TotalDays * SegmentsPerDay; i++)
            {
                float x = i * SegmentWidth;
                bool endOfWeek = (i % (SegmentsPerDay * DaysPerWeek)) == 0;
                bool endOfDay = (i % SegmentsPerDay) == 0;
                group.Children.Add(Line(x, 0, x, endOfDay ? DataHeight : ChartHeight, endOfWeek ? 2.5f : 1f));
            }

            group.Children.Add(Line(0, ChartHeight + DayRow, ChartWidth, ChartHeight + DayRow));

            var chartDataGroup = GetChartDataGroup(0, 0, data);
            chartDataGroup.ClipPath = new Uri(string.Format("url(#{0})", clip.ID), UriKind.Relative);
            group.Children.Add(chartDataGroup);
            group.Children.Add(GetDaysDataGroup(0, ChartHeight, data.First().Time.Date));

            return group;
        }

        private static SvgGroup GetChartDataGroup(float left, float top, IReadOnlyList<DataPoint> data)
        {
            var first = data.First();
            var start = first.Time.Date.At(new LocalTime(0, 0));

            SvgGroup group = Group(left, top);
            for (int i = 0; i < data.Count; i++)
            {
                var point = data[i];
                var cx = GetPointX(start, point);
                var cy = GetPointY(point);
                group.Children.Add(Circle(cx, cy));
                if (i > 0)
                {
                    var previous = data[i - 1];
                    var x = GetPointX(start, previous);
                    var y = GetPointY(previous);
                    var line = Line(x, y, cx, cy);
                    group.Children.Add(line);
                }
            }
            return group;
        }

        private static float GetPointX(LocalDateTime start, DataPoint point)
        {
            return (float)(Period.Between(start, point.Time.LocalDateTime).ToDuration().ToTimeSpan().TotalDays * DayWidth);
        }

        private static int GetPointY(DataPoint point)
        {
            return MaxPef - point.Pef;
        }

        private static SvgGroup GetDaysDataGroup(float left, float top, LocalDate start)
        {
            SvgGroup group = Group(left, top);
            for (int i = 0; i < DaysPerWeek * Weeks; i++)
            {
                var day = start.PlusDays(i);

                var x = 38 + i * DayWidth;
                group.Children.Add(Text(x, DayRow / 2f, -90, day.IsoDayOfWeek.ToString()));
                group.Children.Add(Text(x, DayRow + DateRow / 2f, -90, day.ToString()));
            }
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
            for (int i = 0; i < TotalDays; i += 2)
            {
                float x = DayWidth * i;
                group.Children.Add(Rect(x, 0, DayWidth, ChartHeight, Color.Cyan));
            }
            for (int i = 0; i <= MaxPef; i += PefIncrement)
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
                int x = i * SegmentWidth;
                group.Children.Add(Line(x, 0, x, ChartHeight));
            }

            for (int i = 0; i <= MaxPef; i += PefIncrement)
            {
                int pos = MaxPef - i;
                if (i % 50 == 0 && i != 0)
                {
                    group.Children.Add(Rect(0, pos - PefIncrement, SideColumnWidth, PefIncrement * 2));
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

        private static SvgVisualElement Line(float x1, float y1, float x2, float y2)
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
                X = new SvgUnitCollection() { x },
                Y = new SvgUnitCollection() { y },
                FontSize = new SvgUnit(16)
            };
            svgText.Nodes.Add(new SvgContentNode() { Content = text });
            return svgText;
        }

        private static SvgText Text(float x, float y, float rotate, string text)
        {
            var svgText = new SvgText()
            {
                TextAnchor = SvgTextAnchor.Middle,
                Fill = new SvgColourServer(Color.Black),
                FontSize = new SvgUnit(16)
            };
            svgText.Nodes.Add(new SvgContentNode() { Content = text });
            svgText.Transforms.Add(new SvgTranslate(x, y));
            svgText.Transforms.Add(new SvgRotate(rotate));
            return svgText;
        }

        private static SvgCircle Circle(float cx, float cy)
        {
            return new SvgCircle()
            {
                CenterX = cx,
                CenterY = cy,
                Fill = LineColor,
                Radius = new SvgUnit(5)
            };
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
            return new SvgTransformCollection() { new SvgTranslate(left, top) };
        }
    }
}
