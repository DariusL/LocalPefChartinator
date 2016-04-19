using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalPefChartinator.Util
{
    public class Config
    {
        public static String Get(string key)
        {
            var fromConfig = ConfigurationManager.AppSettings[key];
            return String.IsNullOrWhiteSpace(fromConfig) ? Environment.GetEnvironmentVariable(key) : fromConfig;
        }

        public const string KeyHtml2Pdf = "KEY_HTML_2_PDF";
    }
}
