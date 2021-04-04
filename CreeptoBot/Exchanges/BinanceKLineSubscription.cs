namespace StrategyTester.Exchanges
{
    public record BinanceKLineSubscription
    {
        public string Symbol { get; set; }

        public string Interval { get; set; }

        public override string ToString()
            => $"{Symbol.ToLowerInvariant()}@kline_{Interval}";
    }
}