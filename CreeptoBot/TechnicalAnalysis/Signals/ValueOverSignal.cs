using System.Collections.Generic;

namespace StrategyTester.TechnicalAnalysis.Signals
{
    public class ValueOverSignal : ISignal
    {
        private readonly IIndicator _indicator;
        private readonly IIndicator _indicator1;

        public ValueOverSignal(IIndicator indicator, IIndicator indicator1)
        {
            _indicator = indicator;
            _indicator1 = indicator1;
        }

        public bool Calculate(IReadOnlyList<Candle> candles, int index)
            => _indicator.Calculate(candles, index) > _indicator1.Calculate(candles, index);
    }
}
