using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tavisca.Libraries.Logging.Tests.Utilities;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Libraries.Logging.Tests.Logging
{
    public class FirehoseSinkTest
    {
        [Fact]
        public void Should_Log_Api_Log()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            ILogFormatter formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();

            var logWriter = new LogWriter(formatter, firehoseSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            logData.TryGetValue("id", out esLogId);

            Assert.Equal(id, esLogId);
        }
    }
}
