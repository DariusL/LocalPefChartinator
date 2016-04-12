using System;
using System.IO;
using System.Linq;
using CommandLine;

namespace LocalPefChartinator
{
    public class Program
    {
        enum OutputFormat
        {
            Svg,
            Html,
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
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("format", format, null);
            }
        }

        private static void WriteSvg(string svg, string file)
        {
            File.WriteAllText(file, svg);
        }
    }
}
