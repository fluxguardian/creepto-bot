using System.Text.Json.Serialization;

namespace StrategyTester.Exchanges
{
    public class BinanceKlineResponse
    {
        [JsonPropertyName("e")]
        public string EventType { get; set; }

        [JsonPropertyName("E")]
        public ulong EventTime { get; set; }

        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("k")]
        public KLine KLine { get; set; }
    }

    public class KLine
    {
        [JsonPropertyName("t")]
        public long StartTime { get; set; }

        [JsonPropertyName("T")]
        public long CloseTime { get; set; }

        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("i")]
        public string Internal { get; set; }

        [JsonPropertyName("o")]
        public decimal OpenPrice { get; set; }

        [JsonPropertyName("c")]
        public decimal ClosePrice { get; set; }

        [JsonPropertyName("h")]
        public decimal HighPrice { get; set; }

        [JsonPropertyName("l")]
        public decimal LowPrice { get; init; }

        [JsonPropertyName("v")]
        public decimal BaseAssetVolume { get; set; }

        [JsonPropertyName("n")]
        public int NumberOfTrades { get; set; }

        [JsonPropertyName("x")]
        public bool IsClosed { get; set; }

        [JsonPropertyName("q")]
        public decimal QuoteAssetVolume { get; set; }

        //[JsonPropertyName("V")]
        //public decimal TakerBuyBaseAsset { get; init; }

        //[JsonPropertyName("Q")]
        //public decimal TakerBuyQuoteAsset { get; init; }
    }
}
