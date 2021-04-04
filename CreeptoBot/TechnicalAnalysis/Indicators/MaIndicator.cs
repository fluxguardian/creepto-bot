using StrategyTester.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StrategyTester.TechnicalAnalysis.Indicators
{
    public class MaIndicator : IIndicator
    {
        private readonly int _barCount;
        private readonly Func<Candle, decimal> _selectorFunc;

        public MaIndicator(int barCount, Expression<Func<Candle, decimal>> selectorExpression)
        {
            _barCount = barCount;
            _selectorFunc = selectorExpression.Compile();
        }

        public decimal Calculate(IReadOnlyList<Candle> candles, int index)
            => index - (_barCount - 1) >= 0 ? candles.TakeRange(index - _barCount, index).Average(_selectorFunc.Invoke) : 0;
    }
}
