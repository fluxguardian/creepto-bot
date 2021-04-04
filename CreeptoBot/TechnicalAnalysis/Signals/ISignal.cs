using System.Collections.Generic;

namespace StrategyTester.TechnicalAnalysis.Signals
{
    public interface ISignal
    {
        bool Calculate(IReadOnlyList<Candle> candles, int index);
    }
}