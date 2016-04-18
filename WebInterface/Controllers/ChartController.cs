using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using LocalPefChartinator;

namespace WebInterface.Controllers
{
    public class ChartController : ApiController
    {
        [HttpPost]
        [Route("api/chart")]
        public HttpResponseMessage GenerateChart([FromBody] IEnumerable<SerializedDataPoint> data, string format, string timezone = "Z")
        {
            var deserialized = DataParser.Parse(data, timezone);
            var parsedFormat = ChartWriter.ParseFormat(format);
            var stream = ChartWriter.Stream(deserialized, parsedFormat);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream),

            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(parsedFormat));
            return httpResponseMessage;
        }

        private static string GetContentType(ChartWriter.OutputFormat format)
        {
            switch (format)
            {
                case ChartWriter.OutputFormat.Svg:
                    return "image/svg+xml";
                case ChartWriter.OutputFormat.Html:
                    return "text/html";
                case ChartWriter.OutputFormat.Pdf:
                    return "application/pdf";
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
