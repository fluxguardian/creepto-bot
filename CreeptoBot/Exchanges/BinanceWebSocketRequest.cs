using System.Text.Json.Serialization;

namespace StrategyTester.Exchanges
{
    public record BinanceWebSocketRequest
    {
        [JsonPropertyName("method")]
        public string Method { get; init; }

        [JsonPropertyName("params")]
        public string[] Parameters { get; init; }

        [JsonPropertyName("id")]
        public int Id { get; init; }
    }
}
