using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Common.Plugins.Redis;
using Tavisca.Libraries.Logging.Tests.Utilities;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Libraries.Logging.Tests.Logging
{
    public class CompositeSinkTest
    {
        [Fact]
        public void Should_Log_Api_Log_Using_Redis_Primary()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            var logSource = string.Empty;
            logData.TryGetValue("id", out esLogId);
            logData.TryGetValue("log_source", out logSource);

            Assert.Equal(id, esLogId);
            Assert.Equal("redis", logSource);
        }

        [Fact]
        public void Should_Log_Api_Log_Using_Firehose_Primary()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, firehoseSink, redisSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            var logSource = string.Empty;
            logData.TryGetValue("id", out esLogId);
            logData.TryGetValue("log_source", out logSource);

            Assert.Equal(id, esLogId);
            Assert.Null(logSource);
        }

        [Fact]
        public void Should_Log_Api_Log_Using_Redis_Secondary()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = new FirehoseSink(null);
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, firehoseSink, redisSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            var logSource = string.Empty;
            logData.TryGetValue("id", out esLogId);
            logData.TryGetValue("log_source", out logSource);

            Assert.Equal(id, esLogId);
            Assert.Equal("redis", logSource);
        }

        [Fact]
        public void Should_Log_Api_Log_Using_Firehose_Secondary()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisLogSettings = new RedisLogSettings();
            var redisSink = new RedisSink(redisLogSettings);
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            var logSource = string.Empty;
            logData.TryGetValue("id", out esLogId);
            logData.TryGetValue("log_source", out logSource);

            Assert.Equal(id, esLogId);
            Assert.Null(logSource);
        }
    }
}
