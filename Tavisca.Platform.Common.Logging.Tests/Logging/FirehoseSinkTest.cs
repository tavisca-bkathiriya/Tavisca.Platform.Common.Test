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
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            logData.TryGetValue("id", out esLogId);

            Assert.Equal(id, esLogId);
        }

        [Fact]
        public void Should_Log_Cross_Account_Api_Log()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            ILogFormatter formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetCrossAccountFirehoseSink();

            var logWriter = new LogWriter(formatter, firehoseSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            logData.TryGetValue("id", out esLogId);

            Assert.Equal(id, esLogId);
        }



        [Fact]
        public void Should_Log_Supportable_Datatype_Using_TrySetValue()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            object dateTimeValue = DateTime.Now;
            apiLog.TrySetValue("dateTimeType", dateTimeValue);

            ILogFormatter formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();

            var logWriter = new LogWriter(formatter, firehoseSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);

            string actualDateTimeValue;
            logData.TryGetValue("dateTimeType", out actualDateTimeValue);
            Assert.Equal(Convert.ToString(dateTimeValue), actualDateTimeValue);
        }


        [Fact]
        public void Should_Log_Trace_Log()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var traceLog = Utility.GetTraceLog();
            traceLog.Id = id;
            ILogFormatter formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();

            var logWriter = new LogWriter(formatter, firehoseSink);
            logWriter.WriteAsync(traceLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            logData.TryGetValue("id", out esLogId);
            Assert.Equal(id, esLogId);
        }

        [Fact]
        public void Should_Log_Exception_Log()
        {
            try
            {
                throw new ArgumentNullException();
            }
            catch (Exception exception)
            {
                var id = Convert.ToString(Guid.NewGuid());
                var apiLog = Utility.GetApiLog();
                apiLog.Id = id;
                var exceptionLog = GetErrorEntry(exception, apiLog);
                ILogFormatter formatter = JsonLogFormatter.Instance;
                var firehoseSink = Utility.GetFirehoseSink();
                var logWriter = new LogWriter(formatter, firehoseSink);
                logWriter.WriteAsync(exceptionLog).GetAwaiter().GetResult();

                var logData = Utility.GetEsLogDataById(id, extraRetryCount: 10);
                var esLogId = string.Empty;
                logData.TryGetValue("id", out esLogId);
                Assert.Equal(id, esLogId);
            }
        }

        private ExceptionLog GetErrorEntry(Exception exception, ILog log)
        {
            var exceptionLog = new ExceptionLog(exception);

            var baseLog = log as LogBase;
            if (baseLog == null)
            {
                return exceptionLog;
            }

            exceptionLog.AppDomain = baseLog.AppDomain;
            exceptionLog.ApplicationName = baseLog.ApplicationName;
            exceptionLog.Id = baseLog.Id;
            return exceptionLog;
        }
    }
}
