using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tavisca.Libraries.Logging.Tests.Utilities
{
    public class EsLogReader
    {
        private ElasticClient _esClient;

        public EsLogReader(string esUrl)
        {
            var uri = new Uri(esUrl);
            var pool = new SingleNodeConnectionPool(uri);
            var settings = new ConnectionSettings(pool, sourceSerializer: JsonNetSerializer.Default);
            settings.DisableDirectStreaming();
            _esClient = new ElasticClient(settings);
        }

        public Dictionary<string, string> GetLog(string index, string query, int extraRetryCount = 0)
        {
            var logData = new Dictionary<string, string>();
            var retryCount = 0;
            retryCount = retryCount - extraRetryCount;
            while (retryCount <= 10)
            {
                Thread.Sleep(30000);
                try
                {
                    var esResponse = _esClient.Search<object>(s => s
                                .AllTypes()
                                .Index(index)
                                .Size(10)
                                .Query(
                                    q =>
                                        q.DateRange(
                                            x =>
                                                x.GreaterThanOrEquals(DateMath.Anchored(DateTime.UtcNow.AddSeconds(-300)))
                                                    .LessThanOrEquals(DateMath.Anchored(DateTime.UtcNow))
                                                    .Field("log_time")) &&
                                        q.QueryString(x => x.Query(query).AnalyzeWildcard()
                                            )
                                ));

                    if (esResponse.IsValid == true)
                    {
                        var hit = esResponse.Hits.ToList().First();
                        List<JToken> tokens = ((JObject)hit.Source).Children().ToList();
                        foreach (var token in tokens)
                        {
                            logData.Add(token.Path, token.Children().First().ToString());
                        }
                        break;
                    }

                }
                catch (Exception ex)
                {
                    //throw new Exception("Problem while reading from ES");
                }
                finally
                {
                    retryCount += 1;
                }
            } 
            if(logData.Count == 0)
                throw new Exception("Problem while reading from ES");
            return logData;
        }
    }
}
