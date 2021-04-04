using StrategyTester.TechnicalAnalysis.Indicators;
using System.Collections.Generic;

namespace StrategyTester.TechnicalAnalysis.Signals
{
    internal class ValueUnderSignal : ISignal
    {
        private readonly IIndicator _indicator;
        private readonly IIndicator _indicator1;

        public ValueUnderSignal(IIndicator indicator, IIndicator indicator1)
        {
            _indicator = indicator;
            _indicator1 = indicator1;
        }

        public bool Calculate(IReadOnlyList<Candle> candles, int index)
            => _indicator.Calculate(candles, index) < _indicator1.Calculate(candles, index);
    }
}
