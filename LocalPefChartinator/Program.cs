using System;
using System.IO;
using System.Linq;
using CommandLine;

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
        }

        public static void Main(string[] args)
        {
            Options options = new Options();
            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options, () => { }))
            {
                var values = 
                    File.ReadLines(options.InputFile)
                    .Select(line => line.Split(','))
                    .Select(array => new Tuple<string, string>(array[0], array[1]));

                var parsed = DataParser.Parse(values, options.TimeZone);

                File.WriteAllText(options.OutputFile, ChartGenerator.Generate(parsed));
            }
        }
    }
}
