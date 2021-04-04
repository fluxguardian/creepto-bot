using StrategyTester.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StrategyTester.TechnicalAnalysis.Indicators
{
    public class PercentageDifferenceIndicator : IIndicator
    {
        private readonly Func<Trade> _tradeFunc;

        public PercentageDifferenceIndicator(Func<Trade> tradeFunc)
        {
            _tradeFunc = tradeFunc;
        }
        
        //public decimal Calculate(IndicatorContext indicatorContext)
        //{

        //}
        public decimal Calculate(IReadOnlyList<Candle> candles, int index)
        {
            var trade = _tradeFunc.Invoke();
            if (trade == null)
            {
                return 0;
            }

            // TODO: Allow user to select what candle field to be used
            return MathExtensions.PercentageDifference(trade.Price, candles[index].Close);
    }
    }
}
