using System.Text.Json.Serialization;

namespace TrademarkOfficer.Models
{
    internal class TrademarkResponse
    {
        [JsonPropertyName("hits")]
        public HitResult Result { get; set; } = null!;

        public class HitResult
        {
            [JsonPropertyName("hits")]
            public IEnumerable<Hit> Hits { get; set; } = [];
        }

        public class Hit
        {
            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("source")]
            public Source SourceInfo { get; set; } = null!;
        }

        public class Source
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("alive")]
            public bool Alive { get; set; }

            [JsonPropertyName("filedDate")]
            public DateTime FiledDate { get; set; }

            [JsonPropertyName("goodsAndServices")]
            public IEnumerable<string> GoodsAndServices { get; set; } = [];

            [JsonPropertyName("markType")]
            public IEnumerable<string> MarkTypes { get; set; } = [];

            [JsonPropertyName("ownerName")]
            public IEnumerable<string> OwnerNames { get; set; } = [];

            [JsonPropertyName("ownerFullText")]
            public IEnumerable<string> OwnerFullTexts { get; set; } = [];

            [JsonIgnore]
            public string OwnerName => OwnerNames.Any() ? string.Join(", ", OwnerNames) : string.Empty;
        }
    }
}