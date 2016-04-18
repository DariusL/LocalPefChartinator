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
            if (!Parser.Default.ParseArgumentsStrict(args, options, () => { }))
            {
                return;
            }
            ChartWriter.OutputFormat format;
            try
            {
                format = ChartWriter.ParseFormat(options.OutputFormat);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            var values =
                File.ReadLines(options.InputFile)
                    .Select(line => line.Split(','))
                    .Select(array => new SerializedDataPoint() {Time = array[0], Pef = array[1]});

            var parsed = DataParser.Parse(values, DataParser.ParseTimeZone(options.TimeZone))
                .OrderBy(value => value.Time)
                .ToArray();

            Write(parsed, format, options.OutputFile);
        }

        private static void Write(IReadOnlyList<DataPoint> points, ChartWriter.OutputFormat format, string file)
        {
            var writer = new ChartWriter("template.html");
            var stream = writer.Stream(points, format);
            File.Delete(file);
            stream.CopyTo(File.OpenWrite(file));
        }
    }
}
