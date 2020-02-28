using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Libraries.Logging.Tests.Utilities;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Libraries.Logging.Tests.Filters
{
    public class FilterImplementationTest
    {
        [Fact]
        public void Should_Filter_Api_Key()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("ApiKey", Utility.CreateMapWithValue("nextgen-apikey", "7867YIUUI"));
            var filter = new ApiKeyFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "nextgen-apikey=786******UUI";
            string actual;
            logData.TryGetValue("ApiKey", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Filter_Credit_Card()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("CreditInfo", Utility.CreateMapWithValue("CreditCardNo", "4444555555555555"));
            var filter = new PaymentDataFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "CreditCardNo=444455******5555";
            string actual;
            logData.TryGetValue("CreditInfo", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Filter_AwsArn()
        {
            string awsArn = "arn:aws:kafka:us-east-1:123456789012:cluster:example-cluster-name:0203456a-abcd-1234-cdef-3be56f8c54ce-2";
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("AwsKey", awsArn);
            var filter = new AwsArnFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            string actual;
            logData.TryGetValue("AwsKey", out actual);
            var arnSections = awsArn.Split(':');
            var expected = awsArn.Replace(arnSections[4], arnSections[4].Substring(0, 2)
                       + new string('*', arnSections[4].Length - 4) + arnSections[4].Substring(arnSections[4].Length - 2));

            Assert.Equal(expected, actual);
        }
    }
}
