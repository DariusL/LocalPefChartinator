using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LocalPefChartinator.Util;
using NodaTime;

namespace LocalPefChartinator
{
    public class ChartWriter
    {
        public enum OutputFormat
        {
            Svg,
            Html,
            Pdf,
            Invalid
        }

        public static OutputFormat ParseFormat(string formatString)
        {
            switch (formatString)
            {
                case "svg":
                    return OutputFormat.Svg;
                case "html":
                    return OutputFormat.Html;
                case "pdf":
                    return OutputFormat.Pdf;
                default:
                    return OutputFormat.Invalid;
            }
        }

        public static Stream Stream(IReadOnlyList<DataPoint> points, OutputFormat format)
        {
            Stream stream;
            switch (format)
            {
                case OutputFormat.Svg:
                    stream = StreamSvg(points);
                    break;
                case OutputFormat.Html:
                    stream = StreamHtml(points);
                    break;
                case OutputFormat.Pdf:
                    stream = StreamPdf(points);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("format", format, null);
            }
            return stream;
        }

        private static string GenerateSvg(IReadOnlyList<DataPoint> points)
        {
            return ChartGenerator.Generate(points);
        }

        private static string GenerateHtml(IReadOnlyList<DataPoint> points)
        {
            var content = String.Join(Environment.NewLine, Group(points)
                .Select(GenerateSvg)
                .Select(SvgForPrint));
            string template = File.ReadAllText("template.html");
            return template.Replace("<!--content-->", content);
        }

        private static string SvgForPrint(string svg)
        {
            // because Svg uses invariant culture when XML'ing
            var lineEnd = svg.IndexOf(Environment.NewLine, StringComparison.InvariantCulture);
            return svg.Substring(lineEnd + Environment.NewLine.Length);
        }

        private static Stream StreamPdf(IReadOnlyList<DataPoint> points)
        {
            string key = "8393e676-9660-4fc0-a9c2-674a3614f650";
            string html = GenerateHtml(points);
            using (var client = new WebClient())
            {
                NameValueCollection options = new NameValueCollection { { "apikey", key }, { "value", html } };

                return new MemoryStream(client.UploadValues("http://api.html2pdfrocket.com/pdf", options));
            }
        }

        private static Stream StreamSvg(IReadOnlyList<DataPoint> points)
        {
            return Stream(GenerateSvg(points));
        }

        private static Stream StreamHtml(IReadOnlyList<DataPoint> points)
        {
            return Stream(GenerateHtml(points));
        }

        private static Stream Stream(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        public static IEnumerable<IReadOnlyList<DataPoint>> Group(IReadOnlyList<DataPoint> data)
        {
            LocalDate pageEnd = data.First().Time.Date.PlusDays(ChartGenerator.TotalDays);
            List<DataPoint> temp = new List<DataPoint>();
            foreach (var point in data)
            {
                if (!point.Time.Date.IsBefore(pageEnd))
                {
                    yield return temp;
                    temp = new List<DataPoint>() { point };
                    pageEnd = pageEnd.PlusDays(ChartGenerator.TotalDays);
                }
                else
                {
                    temp.Add(point);
                }
            }
            yield return temp;
        }
    }
}
