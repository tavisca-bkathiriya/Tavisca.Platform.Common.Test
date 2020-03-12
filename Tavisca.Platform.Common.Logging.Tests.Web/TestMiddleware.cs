using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Standard.Libraries.Logging.Tests.Web
{
    public class TestMiddleware
    {
        private readonly RequestDelegate _next;
        public TestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            throw new BaseApplicationException("Error code: 4xx", "Error message:Test Error message", System.Net.HttpStatusCode.BadRequest);
        }
    }
}
