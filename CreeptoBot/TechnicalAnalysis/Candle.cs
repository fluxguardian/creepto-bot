using StrategyTester.Extensions;
using System;

namespace StrategyTester.TechnicalAnalysis
{
    public record Candle()
    {
        public long OpenTime { get; init; }

        public DateTime OpenDate => OpenTime.ToDateTime();

        public decimal Open { get; init; }

        public decimal High { get; init; }

        public decimal Low { get; init; }

        public decimal Close { get; init; }

        public decimal Volume { get; init; }

        public long CloseTime { get; init; }

        public DateTime CloseDate => CloseTime.ToDateTime();

        public decimal QuoteAssetVolume { get; init; }

        public int NumberOfTrades { get; init; }

        public decimal TakerBuyBaseAssetVolume { get; init; }

        public decimal TakerBuyQuoteAssetVolume { get; init; }
    }
}