using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CommandLine;
using LocalPefChartinator.Util;
using NodaTime;

namespace LocalPefChartinator
{
    public class Program
    {
        enum OutputFormat
        {
            Svg,
            Html,
            Pdf,
            Invalid
        }

        class Options
        {
            [Option("in", Required = true, HelpText = "Input csv file")]
            public string InputFile { get; set; }

            [Option("out", Required = false, DefaultValue = "out.svg", HelpText = "Output svg file")]
            public string OutputFile { get; set; }

            [Option("tz", Required = false, DefaultValue = "Z", HelpText = "Timezone offset, example: +03:00")]
            public string TimeZone { get; set; }

            [Option("format", Required = false, DefaultValue = "svg", HelpText = @"Output format, ""svg"" or ""html""")]
            public string OutputFormat { get; set; }
        }

        public static void Main(string[] args)
        {
            Options options = new Options();
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options, () => { }))
            {
                return;
            }
            OutputFormat format = ParseFormat(options.OutputFormat);
            if (format == OutputFormat.Invalid)
            {
                Console.WriteLine("Invalid output format " + options.OutputFormat);
                return;
            }
            var values = 
                File.ReadLines(options.InputFile)
                    .Select(line => line.Split(','))
                    .Select(array => new Tuple<string, string>(array[0], array[1]));

            var parsed = DataParser.Parse(values, options.TimeZone)
                .OrderBy(value => value.Time)
                .ToArray();
                
            Write(parsed, format, options.OutputFile);
        }

        private static OutputFormat ParseFormat(string formatString)
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

        private static void Write(IReadOnlyList<DataPoint> points, OutputFormat format, string file)
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
            File.Delete(file);
            stream.CopyTo(File.OpenWrite(file));
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
                    temp = new List<DataPoint>(){point};
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
