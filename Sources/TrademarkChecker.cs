using Serilog;
using System.Net.Http.Json;
using TrademarkOfficer.Models;

namespace TrademarkOfficer
{
    internal class TrademarkChecker
    {
        internal static async Task CheckTrademarkAsync(string trademarkName, ILogger logger)
        {
            bool hasComDomain = await CheckDomainAsync(trademarkName + ".com");
            bool isRegisteredInUs = await CheckTrademarkInUsAsync(trademarkName, logger);
            if (hasComDomain || isRegisteredInUs)
            {
                if (hasComDomain)
                {
                    logger.Warning("Trademark '{TrademarkName}' already has a .com domain registered.", trademarkName);
                }
                if (isRegisteredInUs)
                {
                    logger.Warning("Trademark '{TrademarkName}' is already registered in the US.", trademarkName);
                }
            }
            else
            {
                logger.Information("Trademark '{TrademarkName}' is available for registration and does not have a .com domain.", trademarkName);
            }
        }

        private static async Task<bool> CheckDomainAsync(string domainName)
        {
            string url = "https://rdap.godaddy.com/v1/domain/" + domainName.ToUpperInvariant();
            using HttpClient client = new();
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        private static async Task<bool> CheckTrademarkInUsAsync(string trademarkName, ILogger logger)
        {
            const int minScore = 10;
            const string url = "https://tmsearch.uspto.gov/api-v1-0-0/tmsearch";
            const string queryTemplate = "{\"query\":{\"bool\":{\"must\":[{\"bool\":{\"should\":[{\"query_string\":{\"query\":\"{{Trademark}}*\"," +
                "\"default_operator\":\"AND\",\"fields\":[\"goodsAndServices\",\"markDescription\",\"ownerName\",\"translate\"," +
                "\"wordmark\",\"wordmarkPseudoText\"]}},{\"term\":{\"WM\":{\"value\":\"{{Trademark}}\",\"boost\":6}}},{\"match_phrase\":" +
                "{\"WMP5\":{\"query\":\"{{Trademark}}\"}}},{\"query_string\":{\"query\":\"{{Trademark}}\",\"default_operator\":\"AND\",\"fields\":" +
                "[\"goodsAndServices\",\"markDescription\",\"ownerName\",\"translate\",\"wordmark\",\"wordmarkPseudoText\"]}}," +
                "{\"term\":{\"SN\":{\"value\":\"{{Trademark}}\"}}},{\"term\":{\"RN\":{\"value\":\"{{Trademark}}\"}}}]}}],\"filter\":[{\"term\":" +
                "{\"LD\":\"true\"}}]}},\"aggs\":{\"alive\":{\"terms\":{\"field\":\"alive\"}},\"cancelDate\":{\"value_count\":" +
                "{\"field\":\"cancelDate\"}}},\"size\":100,\"from\":0,\"track_total_hits\":true,\"highlight\":{\"fields\":{" +
                "\"abandonDate\":{},\"attorney\":{},\"alive\":{},\"cancelDate\":{},\"coordinatedClass\":{},\"currentBasis\":{}," +
                "\"drawingCode\":{},\"filedDate\":{},\"goodsAndServices\":{},\"id\":{},\"internationalClass\":{},\"markDescription\":{}," +
                "\"markType\":{},\"ownerFullText\":{},\"ownerName\":{},\"ownerType\":{},\"priorityDate\":{},\"registrationDate\":{}," +
                "\"registrationId\":{},\"registrationType\":{},\"supplementalRegistrationDate\":{},\"translate\":{},\"usClass\":{}," +
                "\"wordmarkPseudoText\":{}},\"pre_tags\":[\"<strong>\"],\"post_tags\":[\"</strong>\"],\"number_of_fragments\":0}," +
                "\"_source\":[\"abandonDate\",\"alive\",\"attorney\",\"cancelDate\",\"coordinatedClass\",\"currentBasis\",\"drawingCode\"," +
                "\"filedDate\",\"goodsAndServices\",\"id\",\"internationalClass\",\"markDescription\",\"markType\",\"ownerFullText\"," +
                "\"ownerName\",\"ownerType\",\"priorityDate\",\"registrationDate\",\"registrationId\",\"registrationType\"," +
                "\"supplementalRegistrationDate\",\"translate\",\"usClass\",\"wordmark\",\"wordmarkPseudoText\"],\"min_score\":{{MinScore}}}";
            string request = queryTemplate.Replace("{{Trademark}}", trademarkName).Replace("{{MinScore}}", minScore.ToString());
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36");
            var response = await client.PostAsJsonAsync(url, request);
            TrademarkResponse result = await response.Content.ReadFromJsonAsync<TrademarkResponse>()
                ?? throw new InvalidOperationException("Failed to deserialize the trademark response.");
            foreach (var hit in result.Result.Hits)
            {
                logger.Debug("Found trademark: {TrademarkId} with score {Score}: {Name}", hit.SourceInfo.Id, hit.Score, hit.SourceInfo.OwnerName);
            }
            return result.Result.Hits.Any();
        }
    }


}