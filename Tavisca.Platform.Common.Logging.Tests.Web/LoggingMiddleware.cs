using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.WebApi.Middlewares;
using Tavisca.Platform.Common.Models;
using System.IO;

namespace Tavisca.Standard.Libraries.Logging.Tests.Web
{
    public class LoggingMiddleware : ServiceRootLogMiddlewareBase
    {
        public LoggingMiddleware(RequestDelegate next) : base(next) { }
        //Skip root logs for UI specific APIs
        private static readonly List<string> RouteToSkipLogging = new List<string>
        {
            "api/team",
            "api/calltree",
            "api/monitoring",
            "api/environment",
            "api/event/alarm",
            "api/event/notification",
            "api/messenger/templates",
            "api/messenger/templates/list",
            "api/alarms/migration"
        };
        private static readonly Dictionary<string, Tuple<string, string>> RouteMap = new Dictionary<string, Tuple<string, string>>
        {
            {"api/event", new Tuple<string, string>("event", "notify")},
            {"api/sns/notify", new Tuple<string, string>("sns", "notify")},
            {"api/sns/confirm", new Tuple<string, string>("sns", "confirm")},
            {"api/twilio/calltree", new Tuple<string, string>("twilio", "notify")}
        };
        protected override async Task<ApiLog> GetLog(HttpContext httpContext)
        {
            //var responseBodyStream = new MemoryStream();
            //httpContext.Response.Body = responseBodyStream;

            var urlMapping = GetRouteMapping(httpContext.Request.Path);
            var log = new ApiLog
            {
                ApplicationName = "Test Application",
                Api = urlMapping.Item1,
                Verb = urlMapping.Item2,
                Url = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}{httpContext.Request.Path.Value}",
                IsSuccessful = httpContext.Response?.StatusCode == 200,
                Request = new Payload(await GetRequestPayload(httpContext)),
                Response = new Payload(await GetResponsePayload(httpContext))
                //CorrelationId = BaseContext.Current?.CorrelationId,
                //StackId = BaseContext.Current?.StackId,
                //ApplicationTransactionId = BaseContext.Current?.TransactionId
            };
            foreach (var header in httpContext.Request.Headers)
                log.RequestHeaders[header.Key] = string.Join(", ", header.Value);
            foreach (var header in httpContext.Response.Headers)
                log.ResponseHeaders[header.Key] = string.Join(", ", header.Value);


            return log; //TODO: Manually verify logging on ES
        }

        protected override bool ShouldLog(HttpRequest request, HttpResponse response)
        {

            return true;
        }
        private static Tuple<string, string> GetRouteMapping(string path)
        {
            foreach (var callType in RouteMap)
                if (path.Contains(callType.Key))
                    return callType.Value;
            return new Tuple<string, string>(path, path);
        }
    }
}
