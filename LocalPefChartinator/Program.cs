using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using CommandLine;

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

            var parsed = DataParser.Parse(values, options.TimeZone);
                
            Write(ChartGenerator.Generate(parsed), format, options.OutputFile);
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

        private static void Write(string svg, OutputFormat format, string file)
        {
            switch (format)
            {
                case OutputFormat.Svg:
                    WriteSvg(svg, file);
                    break;
                case OutputFormat.Html:
                    WriteHtml(svg, file);
                    break;
                case OutputFormat.Pdf:
                    WritePdf(svg, file);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("format", format, null);
            }
        }

        private static void WriteSvg(string svg, string file)
        {
            File.WriteAllText(file, svg);
        }

        private static void WriteHtml(string svg, string file)
        {
            string result = SvgToHtml(svg);
            File.WriteAllText(file, result);
        }

        private static void WritePdf(string svg, string file)
        {
            string key = "8393e676-9660-4fc0-a9c2-674a3614f650";
            string html = SvgToHtml(svg);
            using (var client = new WebClient())
            {
                NameValueCollection options = new NameValueCollection();
                options.Add("apikey", key);
                options.Add("value", html);

                var result = client.UploadValues("http://api.html2pdfrocket.com/pdf", options);

                File.WriteAllBytes(file, result);
            }
        }

        private static string SvgToHtml(string svg)
        {
            string template = File.ReadAllText("template.html");
            // because Svg uses invariant culture when XML'ing
            var lineEnd = svg.IndexOf(Environment.NewLine, StringComparison.InvariantCulture);
            svg = svg.Substring(lineEnd + Environment.NewLine.Length);
            return template.Replace("<!--content-->", svg);
        }
    }
}
