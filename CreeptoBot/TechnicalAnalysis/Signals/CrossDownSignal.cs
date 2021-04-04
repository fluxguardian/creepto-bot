using System.Collections.Generic;

namespace StrategyTester.TechnicalAnalysis.Signals
{
    public class CrossDownSignal : ISignal
    {
        private readonly IIndicator _indicator;
        private readonly IIndicator _indicator1;
        private decimal _difference;

        public CrossDownSignal(IIndicator indicator, IIndicator indicator1)
        {
            _difference = 0;
            _indicator = indicator;
            _indicator1 = indicator1;
        }

        public bool Calculate(IReadOnlyList<Candle> candles, int index)
        {
            var previousDiff = _difference;
            _difference = _indicator.Calculate(candles, index) - _indicator1.Calculate(candles, index);

            return _difference < 0 && previousDiff >= 0;
        }
    }
}
