using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Redis;

namespace Tavisca.Standard.Libraries.Logging.Tests.Web.Utilities
{
    public static class Utility
    {
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
    }
}
