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
            ChartWriter.OutputFormat format = ChartWriter.ParseFormat(options.OutputFormat);
            if (format == ChartWriter.OutputFormat.Invalid)
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

            ChartWriter.Write(parsed, format, options.OutputFile);
        }
    }
}
