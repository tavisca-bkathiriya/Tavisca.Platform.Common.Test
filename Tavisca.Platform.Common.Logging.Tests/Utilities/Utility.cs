using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Common.Plugins.Redis;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Libraries.Logging.Tests.Utilities
{
    public static class Utility
    {
        private static readonly string _request = Resource.BookInitRequest;
        private static readonly string _xmlData = Resource.XmlData;
        public static ApiLog GetApiLog()
        {
            var log = new ApiLog
            {
                Api = "api",
                Verb = "verb",
                Request = new Payload(_request),
                Response = new Payload(_xmlData),
                Url = "https://www.google.com?param1=val1&param2=val2",
                IsSuccessful = true,
                TimeTakenInMs = 23324.4556,
                TransactionId = "13124423523",
                ClientIp = GetRandomIpAddress()
            };
            return log;
        }

        public static IPAddress GetRandomIpAddress()
        {
            var data = new byte[4];
            new Random().NextBytes(data);
            IPAddress ip = new IPAddress(data);
            return ip;
        }

        public static IDictionary<string, string> CreateMapWithValue(string mapKey, string mapValue)
        {
            IDictionary<string, string> map = new Dictionary<string, string>();
            if (mapValue == null)
            {
                return null;
            }

            if (mapValue == string.Empty)
            {
                return map;
            }

            map.Add(mapKey, mapValue);
            return map;
        }

        public static Payload CreatePayload()
        {
            var payload = new Payload(_request);
            return payload;
        }

        public static RedisSink GetRedisSink()
        {
            var redisLogSettings = new RedisLogSettings
            {
                ApiSetting = new RedisSetting
                {
                    Hosts = new List<RedisHost> {
                        new RedisHost
                        {
                            Url = "master.travel-qa-logging.l86run.use1.cache.amazonaws.com",
                            Port = "6379",
                            IsSslEnabled = true
                        }
                    },
                    QueueName = "travel-qa-logging-api"
                },
                ExceptionSetting = new RedisSetting
                {
                    Hosts = new List<RedisHost> {
                        new RedisHost
                        {
                            Url = "master.travel-qa-logging.l86run.use1.cache.amazonaws.com",
                            Port = "6379",
                            IsSslEnabled = true
                        }
                    },
                    QueueName = "travel-qa-logging-exception"
                },
                TraceSetting = new RedisSetting
                {
                    Hosts = new List<RedisHost> {
                        new RedisHost
                        {
                            Url = "master.travel-qa-logging.l86run.use1.cache.amazonaws.com",
                            Port = "6379",
                            IsSslEnabled = true
                        }
                    },
                    QueueName = "travel-qa-logging-trace"
                }
            };

            return new RedisSink(redisLogSettings);
        }

        public static Dictionary<string, string> GetEsLogDataById(string id)
        {
            var url = "https://es.qa.cnxloyalty.com";
            var esLogReader = new EsLogReader(url);
            var index = "log*";
            var query = $"id:{id}";
            var logData = esLogReader.GetLog(index, query);
            return logData;
        }

        public static FirehoseSink GetFirehoseSink()
        {
            IFirehoseLogSettingsProvider firehoseLogSettingsProvider = new StaticFireHoseSettingsProvider();
            var firehoseSink = new FirehoseSink(firehoseLogSettingsProvider);
            return firehoseSink;
        }

        public static CompositeSink GetCompositeSink(ILogFormatter logFormatter, ILogSink primarySink, ILogSink secondarySink)
        {
            var compositeSink = new CompositeSink(logFormatter, primarySink, secondarySink);
            return compositeSink;
        }

        public static TraceLog GetTraceLog()
        {
            var log = TraceLog.GenerateTraceLog("Test Trace Log");
            log.Category = "Info";
            return log;
        }
    }
}
