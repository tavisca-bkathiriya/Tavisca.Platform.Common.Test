using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tavisca.Common.Plugins.Redis;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Tavisca.Libraries.Logging.Tests.Utilities;
using Xunit;
using System.Net;
using Tavisca.Platform.Common.ExceptionManagement;
using Tavisca.Platform.Common;

namespace Tavisca.Libraries.Logging.Tests.Logging
{
    public class RedisSinkTest
    {
        public class ErrorHandler : IErrorHandler
        {
            public bool HandleException(Exception ex, string policy, out Exception newException)
            {
                newException = ex;
                return true;
            }
        }

        [Fact]
        public void Should_Log_Api_Log()
        {
            ExceptionPolicy.Configure(new ErrorHandler());
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            ILogFormatter formatter = JsonLogFormatter.Instance;
            var redisSink = Utility.GetRedisSink();

            var logWriter = new LogWriter(formatter, redisSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            ////Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;
            logData.TryGetValue("id", out esLogId);
            Assert.Equal(id, esLogId);            
        }

        [Fact]
        public void Should_Not_Log_Api_Log()
        {
            ExceptionPolicy.Configure(new ErrorHandler());
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            ILogFormatter formatter = JsonLogFormatter.Instance;

            Tavisca.Common.Plugins.Configuration.ConfigurationProvider configProvider;
            lock (Utility._lock)
            {
                configProvider = new Tavisca.Common.Plugins.Configuration.ConfigurationProvider("test_new_app");
            }
            var redisSink = Utility.GetLoggingDisabledRedisSink(configProvider);

            var logWriter = new LogWriter(formatter, redisSink, configurationProvider: configProvider);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            Assert.Throws<Exception>(() => Utility.GetEsLogDataById(id));
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_When_RedisSetting_Is_Null()
        {
            ExceptionPolicy.Configure(new ErrorHandler());
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            Assert.Throws<ArgumentNullException>(() => Utility.GetRedisSinkWithNullSettings());
        }

        [Fact]
        public void Should_Log_Trace_Log()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var traceLog = Utility.GetTraceLog();
            traceLog.Id = id;
            ILogFormatter formatter = JsonLogFormatter.Instance;
            var redisSink = Utility.GetRedisSink();

            var logWriter = new LogWriter(formatter, redisSink);
            logWriter.WriteAsync(traceLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

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
                var redisSink = Utility.GetRedisSink();
                var logWriter = new LogWriter(formatter, redisSink);
                logWriter.WriteAsync(exceptionLog).GetAwaiter().GetResult();

                //Thread.Sleep(40000);

                var logData = Utility.GetEsLogDataById(id);
                var esLogId = string.Empty;
                logData.TryGetValue("id", out esLogId);
                Assert.Equal(id, esLogId);
            }
        }

        //[Fact]
        //public void Should_Log_Exception_Log1()
        //{
        //    var temp = new Class1();
        //    temp.TestMethod();
        //}

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

        [Fact]
        public void Should_Add_Ip_Prefix()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            ILogFormatter formatter = JsonLogFormatter.Instance;
            var redisSink = Utility.GetRedisSink();

            var logWriter = new LogWriter(formatter, redisSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            string esLogId;
            Assert.True(logData.TryGetValue("ip_client_ip", out esLogId)); // Verify ip_ prefix
        }

        [Fact]
        public void Should_Add_Attr_Prefix()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("txid", "13124420000");
            ILogFormatter formatter = JsonLogFormatter.Instance;
            var redisSink = Utility.GetRedisSink();

            var logWriter = new LogWriter(formatter, redisSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            string esLogId;
            Assert.True(logData.TryGetValue("attr_txid", out esLogId)); // Verify ip_ prefix
        }

        [Fact]
        public void Should_Log_Different_Datatypes_With_Required_Prefix()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            var dateTimeValue = DateTime.Now;
            apiLog.SetValue("dateTimeType", dateTimeValue);

            string stringValue = "TestString";
            apiLog.SetValue("stringType", stringValue);

            var geoPointValue = new GeoPoint((decimal)23.11, (decimal)-8.96);
            apiLog.SetValue("geoPointType", geoPointValue);

            int intValue = -100;
            apiLog.SetValue("intType", intValue);

            long longValue = -100100100100100;
            apiLog.SetValue("longType", longValue);

            ulong ulongValue = 100100100100100;
            apiLog.SetValue("ulongType", ulongValue);

            uint uintValue = 100;
            apiLog.SetValue("uintType", uintValue);

            float floatValue = 134.45E-2f;
            apiLog.SetValue("floatType", floatValue);

            double doubleValue = 0.42e2;
            apiLog.SetValue("doubleType", doubleValue);

            decimal decimalValue = 1.5E6m;
            apiLog.SetValue("decimalType", decimalValue);

            bool boolValue = true;
            apiLog.SetValue("boolType", boolValue);

            byte[] byteValue = Utility.CreatePayload().GetBytes();
            apiLog.SetValue("byteType", byteValue);

            Payload payloadValue = Utility.CreatePayload();
            apiLog.SetValue("payloadType", payloadValue);

            IDictionary<string, string> dictionaryvalue = new Dictionary<string, string> { {"hi", "hello" } };
            apiLog.SetValue("dictionaryType", dictionaryvalue);

            Map mapvalue = new Map(dictionaryvalue, MapFormat.Json);
            apiLog.TrySetValue("mapType", mapvalue);

            IPAddress ipAddressValue = Utility.GetRandomIpAddress();
            apiLog.SetValue("ipAddressType", ipAddressValue);

            ILogFormatter formatter = JsonLogFormatter.Instance;
            var redisSink = Utility.GetRedisSink();

            var logWriter = new LogWriter(formatter, redisSink);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);

            string actualDateTimeValue;
            logData.TryGetValue("dateTimeType", out actualDateTimeValue);
            Assert.Equal(Convert.ToString(dateTimeValue), actualDateTimeValue);

            string actualStringValue;
            logData.TryGetValue("stringType", out actualStringValue);
            Assert.Equal(stringValue, actualStringValue);

            string actualGeoPointValue;
            logData.TryGetValue("geo_geoPointType", out actualGeoPointValue);
            var expectedGeoPointValue = "{\r\n  \"lat\": 23.11,\r\n  \"lon\": -8.96\r\n}";
            Assert.Equal(expectedGeoPointValue, actualGeoPointValue);

            string actualIntValue;
            logData.TryGetValue("intType", out actualIntValue);
            Assert.Equal(intValue.ToString(), actualIntValue);

            string actualLongValue;
            logData.TryGetValue("longType", out actualLongValue);
            Assert.Equal(longValue.ToString(), actualLongValue);

            string actualUlongValue;
            logData.TryGetValue("ulongType", out actualUlongValue);
            Assert.Equal(ulongValue.ToString(), actualUlongValue);

            string actualUIntValue;
            logData.TryGetValue("uintType", out actualUIntValue);
            Assert.Equal(uintValue.ToString(), actualUIntValue);

            string actualFloatValue;
            logData.TryGetValue("floatType", out actualFloatValue);
            Assert.Equal(floatValue.ToString(), actualFloatValue);

            string actualDoubleValue;
            logData.TryGetValue("doubleType", out actualDoubleValue);
            Assert.Equal(doubleValue.ToString(), actualDoubleValue);

            string actualDecimalValue;
            logData.TryGetValue("decimalType", out actualDecimalValue);
            Assert.Equal(decimalValue.ToString(), actualDecimalValue);

            string actualBoolValue;
            logData.TryGetValue("boolType", out actualBoolValue);
            Assert.Equal(boolValue.ToString(), actualBoolValue);

            string actualByteValue;
            logData.TryGetValue("byteType", out actualByteValue);
            Assert.NotNull(actualBoolValue);

            string actualPayloadValue;
            logData.TryGetValue("payloadType", out actualPayloadValue);
            Assert.NotNull(actualPayloadValue);

            string actualDictionaryValue;
            logData.TryGetValue("dictionaryType", out actualDictionaryValue);
            var expectedDictionaryValue = "hi=hello";
            Assert.Equal(expectedDictionaryValue, actualDictionaryValue);

            string actualMapValue;
            logData.TryGetValue("json_mapType", out actualMapValue);
            var expectedMapValue = "{\r\n  \"hi\": \"hello\"\r\n}";
            Assert.Equal(expectedMapValue, actualMapValue);

            string actualIpAddressValue;
            logData.TryGetValue("ip_ipAddressType", out actualIpAddressValue);
            Assert.Equal(ipAddressValue.ToString(), actualIpAddressValue);
        }
    }
}
