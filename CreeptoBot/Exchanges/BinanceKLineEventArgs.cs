namespace StrategyTester.Exchanges
{
    public record BinanceKLineEventArgs
    {
        public BinanceKlineResponse KLine { get; init; }
    }
}