using System.Collections.Generic;

namespace StrategyTester.TechnicalAnalysis.Indicators
{
    public class ValueIndicator : IIndicator
    {
        private readonly decimal _value;

        public ValueIndicator(decimal value)
        {
            _value = value;
        }

        public decimal Calculate(IReadOnlyList<Candle> candles, int index)
            => _value;
    }
}
