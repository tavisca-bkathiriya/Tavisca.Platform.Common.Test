using System;
using System.Threading;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;
using Tavisca.Libraries.Logging.Tests.Utilities;

namespace Tavisca.Libraries.Logging.Tests.Logging
{
    public class LoggerTest
    {
        [Fact]
        public void Should_Log_Api_Log()
        {
            try
            {
                var apiLog = Utility.GetApiLog();
                apiLog.Id = "TestId1";
                ILogFormatter formatter = JsonLogFormatter.Instance;
                var redisSink = Utility.GetRedisSink();

                var logWriter = new LogWriter(formatter, redisSink);
                logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
                Thread.Sleep(60000);
            }
            catch (Exception ex)
            {

            }
        }

    }
}
