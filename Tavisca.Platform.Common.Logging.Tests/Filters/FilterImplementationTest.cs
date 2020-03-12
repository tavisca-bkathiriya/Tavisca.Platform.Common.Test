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
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "nextgen-apikey=786******UUI";
            string actual;
            logData.TryGetValue("ApiKey", out actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Not_Filter_Api_Key_Less_Than_Six_Chars()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("ApiKey", Utility.CreateMapWithValue("nextgen-apikey", "ABCDE"));
            var filter = new ApiKeyFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "nextgen-apikey=ABCDE";
            string actual;
            logData.TryGetValue("ApiKey", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Not_Filter_Api_Key_With_Invalid_Key()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("ApiKey", Utility.CreateMapWithValue("-apikey", "7867YIUUI"));
            var filter = new ApiKeyFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "-apikey=7867YIUUI";
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
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "CreditCardNo=444455******5555";
            string actual;
            logData.TryGetValue("CreditInfo", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Filter_Credit_Card_As_Payload()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            var creditCardPayload = new Payload("4444555555555555");
            apiLog.SetValue("CreditInfo", creditCardPayload);
            var filter = new PaymentDataFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "444455******5555";
            string actualUrl;
            logData.TryGetValue("CreditInfo", out actualUrl);
            var actual = Utility.GetOutputFromUrl(actualUrl);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Filter_Credit_Card_As_String()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("CreditInfo", "4444555555555555");
            var filter = new PaymentDataFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "444455******5555";
            string actual;
            logData.TryGetValue("CreditInfo", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Not_Filter_Credit_Card_Less_Than_13_Digits()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("CreditInfo1", "345621234673");
            var filter = new PaymentDataFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected1 = "345621234673";
            string actual1;
            logData.TryGetValue("CreditInfo1", out actual1);

            Assert.Equal(expected1, actual1);
        }

        [Fact]
        public void Should_Not_Filter_Incorrect_Credit_Card()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("CreditInfo2", "1235345621234673");
            apiLog.SetValue("CreditInfo3", "44445555555555552");
            apiLog.SetValue("CreditInfo4", "AAAA555555555555");
            var filter = new PaymentDataFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected2 = "1235345621234673";
            string actual2;
            logData.TryGetValue("CreditInfo2", out actual2);

            var expected3 = "44445555555555552";
            string actual3;
            logData.TryGetValue("CreditInfo3", out actual3);

            var expected4 = "AAAA555555555555";
            string actual4;
            logData.TryGetValue("CreditInfo4", out actual4);

            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
            Assert.Equal(expected4, actual4);
        }

        [Theory]
        [InlineData("arn:aws:kms:us-east-1:123456789012:key")]
        [InlineData("arn:aws:ec2:us-west-2:123456789012:vpc/vpc")]
        [InlineData("arn:aws:ec2:us-west-2:123456789012:vpc:vpc")]
        [InlineData("arn:aws:kafka:us-east-1:123456789012:cluster/example-cluster-name/0203456a-abcd-1234-cdef-3be56f8c54ce-2")]
        [InlineData("arn:aws:kafka:us-east-1:123456789012:cluster/example-cluster-name:0203456a-abcd-1234-cdef-3be56f8c54ce-2")]
        [InlineData("arn:aws:kafka:us-east-1:123456789012:cluster:example-cluster-name:0203456a-abcd-1234-cdef-3be56f8c54ce-2")]
        public void Should_Filter_AwsArn(string awsArn)
        {
            //string awsArn = "arn:aws:kafka:us-east-1:123456789012:cluster:example-cluster-name:0203456a-abcd-1234-cdef-3be56f8c54ce-2";
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
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            string actual;
            logData.TryGetValue("AwsKey", out actual);
            var arnSections = awsArn.Split(':');
            var expected = awsArn.Replace(arnSections[4], arnSections[4].Substring(0, 2)
                       + new string('*', arnSections[4].Length - 4) + arnSections[4].Substring(arnSections[4].Length - 2));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Filter_AwsArn_As_Payload()
        {
            string awsArn = "arn:aws:kafka:us-east-1:123456789012:cluster:example-cluster-name:0203456a-abcd-1234-cdef-3be56f8c54ce-2";
            var awsArnPayload = new Payload(awsArn);
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("AwsKey", awsArnPayload);
            var filter = new AwsArnFilter();
            var filteredLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(filteredLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var arnSections = awsArn.Split(':');
            var expected = awsArn.Replace(arnSections[4], arnSections[4].Substring(0, 2)
                      + new string('*', arnSections[4].Length - 4) + arnSections[4].Substring(arnSections[4].Length - 2));
            string actualUrl;
            logData.TryGetValue("AwsKey", out actualUrl);
            var actual = Utility.GetOutputFromUrl(actualUrl);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Not_Filter_Invalid_AwsArn()
        {
            string awsArn = "aws:kms:us-east-1:123456789012:key";
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
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            string actual;
            logData.TryGetValue("AwsKey", out actual);

            Assert.Equal(awsArn, actual);
        }

        [Fact]
        public void Should_Not_Filter_AwsArn_With_Less_Than_13()
        {
            string awsArn = "aw:km:us:1:k";
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
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            string actual;
            logData.TryGetValue("AwsKey", out actual);

            Assert.Equal(awsArn, actual);
        }
    }
}
