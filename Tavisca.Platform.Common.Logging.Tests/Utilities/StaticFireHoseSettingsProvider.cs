using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Libraries.Logging.Tests.Utilities
{
    public class StaticFireHoseSettingsProvider : IFirehoseLogSettingsProvider
    {
        public async Task<FirehoseLogSettings> GetLogSettingsAsync()
        {
            using (new ProfileContext("fetching firehose log settings"))
            {
                var firehoseLogSettings = new FirehoseLogSettings();
                await Task.Run(() => {
                    firehoseLogSettings.ApiSetting = new FirehoseSettings
                    {
                        Region = "us-east-1",
                        Stream = "travel-qa-logging-api"
                    };
                    firehoseLogSettings.ExceptionSetting = new FirehoseSettings
                    {
                        Region = "us-east-1",
                        Stream = "travel-qa-logging-exception"
                    };
                    firehoseLogSettings.TraceSetting = new FirehoseSettings
                    {
                        Region = "us-east-1",
                        Stream = "travel-qa-logging-trace"
                    };
                });
                return firehoseLogSettings;
            }
        }
    }
}
