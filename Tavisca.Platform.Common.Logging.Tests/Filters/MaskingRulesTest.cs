using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Tavisca.Libraries.Logging.Tests.Utilities;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Logging.Fluent;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Libraries.Logging.Tests.Filters
{
    public class MaskingRulesTest
    {
        [Fact]
        public void Should_Mask_QueryString_Params()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

             List<TextMaskingRule> _rules = new List<TextMaskingRule>();
            _rules.Add(new TextMaskingRule() { Field = "param1", Mask = Masks.MaskCompleteValue });
            _rules.Add(new TextMaskingRule() { Field = "param2", Mask = Masks.MaskCompleteValue });
            var queryStringMaskingRule = new QueryStringMaskingRule("url", _rules.ToArray());
            var filter = new TextLogMaskingFilter(new QueryStringMaskingRule[] { queryStringMaskingRule });
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "https://www.google.com?param1=****&param2=****";
            string actual;
            logData.TryGetValue("url", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Display_MaskingFailed_For_Invalid_Querystring()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            List<TextMaskingRule> _rules = new List<TextMaskingRule>();
            _rules.Add(new TextMaskingRule() { Field = "param1", Mask = Masks.MaskCompleteValue });
            _rules.Add(new TextMaskingRule() { Field = "param2", Mask = Masks.MaskCompleteValue });
            var queryStringMaskingRule = new QueryStringMaskingRule("Verb", _rules.ToArray());
            var filter = new TextLogMaskingFilter(new QueryStringMaskingRule[] { queryStringMaskingRule });
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "Masking Failed";
            string actual;
            logData.TryGetValue("verb", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Mask_TextLog ()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            var filter = new TextLogMaskingFilter(new TextMaskingRule() { Field = "txid", Mask = Masks.DefaultMask});
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "1*********3";
            string actual;
            logData.TryGetValue("txid", out actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Not_Mask_NullOrEmpty_TextLog()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.TransactionId = string.Empty;

            var filter = new TextLogMaskingFilter(new TextMaskingRule() { Field = "txid", Mask = Masks.DoNotMaskNullOrEmpty });
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            string actual;
            logData.TryGetValue("txid", out actual);

            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void MaskCompleteValue_Should_Mast_Entire_Value()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            var filter = new TextLogMaskingFilter(new TextMaskingRule() { Field = "txid", Mask = Masks.MaskCompleteValue });
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "***********";
            string actual;
            logData.TryGetValue("txid", out actual);
                
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Default_Mask_Should_Mask_Value_Based_On_Length()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;
            apiLog.SetValue("test1", "ab");
            apiLog.SetValue("test2", "abcd");
            apiLog.SetValue("test3", "abcdefg");

            var rule1 = new TextMaskingRule()
            {
                Field = "test1",
                Mask = Masks.DefaultMask
            };
            var rule2 = new TextMaskingRule()
            {
                Field = "test2",
                Mask = Masks.DefaultMask
            };
            var rule3 = new TextMaskingRule()
            {
                Field = "test3",
                Mask = Masks.DefaultMask
            };

            var filter = new TextLogMaskingFilter(new List<TextMaskingRule> { rule1, rule2, rule3 });
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected1 = "**";
            string actual1;
            logData.TryGetValue("test1", out actual1);

            var expected2 = "a***";
            string actual2;
            logData.TryGetValue("test2", out actual2);

            var expected3 = "a*****g";
            string actual3;
            logData.TryGetValue("test3", out actual3);

            Assert.Equal(expected1, actual1);
        }

        [Fact]
        public void Should_Mask_Xml()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            var cc = "4111111111111111";
            var maskedCC = "411111******1111";
            var filter = new StreamLogMaskingFilter(new List<PayloadMaskingRule>
            {
                new XmlPayloadMaskingRule("xmlField", new Dictionary<string, string>{ { "oski", "http://oski.io/my_custom_ns" } }, new PayloadFieldMaskingRule[]
                {
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child1/@node1Child1Attr" },
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child1/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child2/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child3/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/oski:node2/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/nodeCC/text()", Mask = Masks.CreditCardMask }
                })
            });

            var xml = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
                            <node1>
                                <node1Child1 node1Child1Attr="" attr_value ""> ab </node1Child1>
                                <node1Child2> abcd </node1Child2>
                                <node1Child3> abcde </node1Child3>
                            </node1>
                            <node2 xmlns=""http://oski.io/my_custom_ns"">uvwxyz</node2>
                            <nodeCC>{0}</nodeCC>
                            <node3>pqrst</node3>
                        </root>", cc);

            apiLog.TrySetValue("xmlField", new Payload(xml));
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(30000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            string actualUrl;
            logData.TryGetValue("xmlField", out actualUrl);
            var actual = Utility.GetOutputFromUrl(actualUrl);

            var expected = string.Format(Regex.Replace(Regex.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
                            <node1>
                                <node1Child1 node1Child1Attr=""a********e"">**</node1Child1>
                                <node1Child2>a***</node1Child2>
                                <node1Child3>a***e</node1Child3>
                            </node1>
                            <node2 xmlns=""http://oski.io/my_custom_ns"">u****z</node2>
                            <nodeCC>{0}</nodeCC>
                            <node3>pqrst</node3>
                        </root>", @"\t|\n|\r", ""), @">\s*<", "><"), maskedCC);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Mask_Json_Payload()
        {
            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            List<PayloadFieldMaskingRule> rules = new List<PayloadFieldMaskingRule>();
            IMask CreditCardMask = new FuncMask(CreditcardMask.Mask);
            rules.Add(new PayloadFieldMaskingRule() { Path = "paymentMethod.cards[0].num", Mask = CreditCardMask });
            var jsonPayloadMaskingRule = new JsonPayloadMaskingRule("request", rules.ToArray());
            var filter = new StreamLogMaskingFilter(new PayloadMaskingRule[] { jsonPayloadMaskingRule });
            Assert.NotNull(filter.Rules);
            var maskedLog = filter.Apply(apiLog);

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink);
            logWriter.WriteAsync(maskedLog).GetAwaiter().GetResult();
            //Thread.Sleep(40000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expectedSubstring = "444455******5555";
            string actualUrl;
            logData.TryGetValue("request", out actualUrl);
            var actual = Utility.GetOutputFromUrl(actualUrl);

            Assert.Contains(expectedSubstring, actual); 
        }

        [Fact]
        public void Should_Be_Able_To_Add_Filters_Using_MaskingDelegate()
        {
            var loggingFilter = new LoggingFilter(new List<ILogFilter>());

            var filter = new TextLogMaskingFilter(new TextMaskingRule() { Field = "txid", Mask = Masks.DefaultMask });
            var filter1 = new TextLogMaskingFilter(new TextMaskingRule() { Field = "verb", Mask = Masks.DefaultMask });

            loggingFilter.ConfigureMaskingDelegate(filter.Apply).Apply();
            loggingFilter.ConfigureMaskingDelegate(filter1.Apply).Apply();

            var id = Convert.ToString(Guid.NewGuid());
            var apiLog = Utility.GetApiLog();
            apiLog.Id = id;

            var formatter = JsonLogFormatter.Instance;
            var firehoseSink = Utility.GetFirehoseSink();
            var redisSink = Utility.GetRedisSink();
            var compositeSink = Utility.GetCompositeSink(formatter, redisSink, firehoseSink);

            var logWriter = new LogWriter(formatter, compositeSink, loggingFilter.Filters);
            logWriter.WriteAsync(apiLog).GetAwaiter().GetResult();
            //Thread.Sleep(60000);

            var logData = Utility.GetEsLogDataById(id);
            var esLogId = string.Empty;

            var expected = "1*********3";
            string actual;
            logData.TryGetValue("txid", out actual);

            var expected1 = "v***";
            string actual1;
            logData.TryGetValue("verb", out actual1);

            Assert.Equal(expected, actual);
            Assert.Equal(expected1, actual1);
        }
    }

    public class LoggingFilter : ILoggingHttpFilter
    {
        public LoggingFilter(List<ILogFilter> filters)
        {
            Filters = filters;
        }

        public List<ILogFilter> Filters { get; }
    }
}
