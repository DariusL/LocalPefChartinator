using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Results;
using LocalPefChartinator;
using NodaTime;

namespace WebInterface.Controllers
{
    public class ChartController : ApiController
    {
        private readonly ChartWriter writer = new ChartWriter(HostingEnvironment.MapPath("~/template.html"));

        [HttpPost]
        [Route("api/chart")]
        public IHttpActionResult GenerateChart([FromBody] List<SerializedDataPoint> data, string format = "", string timezone = "Z")
        {
            if (data == null || data.Count == 0)
            {
                return new BadRequestErrorMessageResult("Missing PEF body", this);
            }
            ChartWriter.OutputFormat parsedFormat;
            try
            {
                parsedFormat = ChartWriter.ParseFormat(format);
            }
            catch (ArgumentException)
            {
                return new BadRequestErrorMessageResult($"Invalid format {format}", this);
            }

            DateTimeZone dateTimeZone;
            try
            {
                dateTimeZone = DataParser.ParseTimeZone(timezone);
            }
            catch (Exception)
            {
                return new BadRequestErrorMessageResult($"Invalid timezone {timezone}", this);
            }
            IReadOnlyList<DataPoint> deserialized;

            try
            {
                deserialized = DataParser.Parse(data, dateTimeZone);
            }
            catch (Exception e)
            {
                return new BadRequestErrorMessageResult(e.Message, this);
            }
            var stream = writer.Stream(deserialized, parsedFormat);
            return WrapStream(stream, parsedFormat);
        }

        private static IHttpActionResult WrapStream(Stream stream, ChartWriter.OutputFormat parsedFormat)
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(parsedFormat));
            return new ResponseMessageResult(httpResponseMessage);
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
