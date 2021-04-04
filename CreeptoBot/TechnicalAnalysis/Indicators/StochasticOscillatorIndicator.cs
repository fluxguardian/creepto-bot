using StrategyTester.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace StrategyTester.TechnicalAnalysis.Indicators
{
    public class StochasticOscillatorIndicator : IIndicator
    {
        private readonly IReadOnlyList<Candle> _candles;

        public StochasticOscillatorIndicator(IReadOnlyList<Candle> candles)
        {
            _candles = candles;
        }

        public StochasticOscillatorIndicator()
        {

        }

        public decimal Calculate()
            => Calculate(0);

        public decimal Calculate(int index)
        {
            var c = _candles.ElementAt(index);
            var close = _candles.ElementAt(index).Close;
            var h14 = _candles.TakeRange(index - 14, index).Max(c => c.High);
            var l14 = _candles.TakeRange(index - 14, index).Min(c => c.Low);

            return 100 * (close - l14) / (h14 - l14);
        }

        public decimal Calculate(IReadOnlyList<Candle> candles, int index)
        {
            var c = candles.ElementAt(index);
            var close = candles.ElementAt(index).Close;
            var h14 = candles.TakeRange(index - 14, index).Max(c => c.High);
            var l14 = candles.TakeRange(index - 14, index).Min(c => c.Low);

            return 100 * (close - l14) / (h14 - l14);
        }
    }
}
