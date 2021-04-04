using StrategyTester.Enums;
using StrategyTester.Technical_Analysis;
using StrategyTester.TechnicalAnalysis;
using StrategyTester.TechnicalAnalysis.Indicators;
using StrategyTester.TechnicalAnalysis.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyTester
{
    public class StrategyFactory
    {
        public Strategy GetStrategy(string name, Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldBuy, Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldSell)
            => name switch
            {
                "madown" => BuildMaDownStrategy(shouldBuy, shouldSell),
                "maup" => BuildMaStrategy(shouldBuy, shouldSell),
                _ => throw new NotImplementedException()
            };

        public Strategy GetStrategy(string name)
            => name switch
            {
                "madown" => BuildMaDownStrategy(
                    (trades, candle) => Task.FromResult(trades.Any() && trades[trades.Count - 1].Direction == TradeDirection.Sell),
                    (trades, candle) => Task.FromResult(!trades.Any() || trades[trades.Count - 1].Direction == TradeDirection.Buy),
                    0.1M
                    ),
                "maup" => BuildMaStrategy(
                    (trades, candle) => Task.FromResult(!trades.Any() || trades[trades.Count - 1].Direction == TradeDirection.Sell),
                    (trades, candle) => Task.FromResult(trades.Any() && trades[trades.Count - 1].Direction == TradeDirection.Buy),
                    500
                    ),
                _ => throw new NotImplementedException()
            };

        private static Strategy BuildMaDownStrategy(Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldBuy, Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldSell, decimal initialInvestment = 500)
        {
            var strategy = new Strategy(
              shouldBuy,
              shouldSell,
              initialInvestment // 0,1 BTC
           );

            var ma7 = new MaIndicator(7, c => c.Close);
            var ma25 = new MaIndicator(25, c => c.Close);
            var stochasticOscillator = new StochasticOscillatorIndicator();

            ValueIndicator value2 = new ValueIndicator(20);
            ValueIndicator valueMinus2 = new ValueIndicator(-20);

            strategy.SellSignal.Set(new ValueOverSignal(stochasticOscillator, new ValueIndicator(95)))
                                .And(new ValueOverSignal(ma7, ma25));

            strategy.BuySignal.Set(new ValueUnderSignal(stochasticOscillator, new ValueIndicator(5)))
                                .And(new ValueUnderSignal(ma7, ma25));

            return strategy;
        }

        private static Strategy BuildMaStrategy(Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldBuy, Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldSell, decimal initialInvestment = 500)
        {
            var strategy = new Strategy(
              shouldBuy,
              shouldSell,
              initialInvestment
           );

            var ma7 = new MaIndicator(7, c => c.Close);
            var ma25 = new MaIndicator(25, c => c.Close);
            var stochasticOscillator = new StochasticOscillatorIndicator();

            // Buy if:
            // - ma7 < ma25 AND
            // - stochasticOscillator < 20
            strategy.BuySignal.Set(new ValueUnderSignal(ma7, ma25)).And(new CrossDownSignal(stochasticOscillator, new ValueIndicator(20)));

            // Sell if:
            // - ma7 > ma25 OR
            // - stochasticOscillator > 80
            strategy.SellSignal.Set(new ValueOverSignal(ma7, ma25)).Or(new ValueOverSignal(stochasticOscillator, new ValueIndicator(80)));

            return strategy;
        }
    }
}
