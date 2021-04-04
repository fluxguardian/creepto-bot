using System.Collections.Generic;

namespace StrategyTester.TechnicalAnalysis
{
    public interface IIndicator
    {
        decimal Calculate(IReadOnlyList<Candle> candles, int index);
    }
}