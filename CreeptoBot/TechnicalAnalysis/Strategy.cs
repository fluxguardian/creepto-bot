using StrategyTester.Enums;
using StrategyTester.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyTester.Technical_Analysis
{
    public class Strategy
    {
        private readonly Func<IReadOnlyList<Trade>, Candle, Task<bool>> _shouldBuy;
        private readonly Func<IReadOnlyList<Trade>, Candle, Task<bool>> _shouldSell;

        public Strategy(Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldBuy, Func<IReadOnlyList<Trade>, Candle, Task<bool>> shouldSell, decimal initialInvestment)
        {

            _shouldBuy = shouldBuy;
            _shouldSell = shouldSell;
            InitialInvestment = initialInvestment;
            Trades = new List<Trade>();
            BuySignal = new SignalEngine();
            SellSignal = new SignalEngine();
        }

        public decimal InitialInvestment { get; private set; }

        public decimal Fee { get; private set; }

        public List<Trade> Trades { get; private set; }

        public decimal ProfitEur { get; private set; }
        public decimal ProfitCoin { get; private set; }
        // move elements above to context class

        public SignalEngine BuySignal { get; set; }

        public SignalEngine SellSignal { get; set; }


        public async Task Execute(IReadOnlyList<Candle> candles)
        {
            var buy = BuySignal.BuildExpression();
            var sell = SellSignal.BuildExpression();

            // TODO: Add live and backtest modes, backtests run the full collection, live only calculates the last candle
            for (int i = 24; i < candles.Count; i++)
            {
                try
                {
                    var candle = candles[i];

                    if (buy.Invoke(candles, i) && 
                        await _shouldBuy.Invoke(Trades, candle))
                    {
                        //How to get buy amount from user?
                        var v = Trades.Any() && Trades.Last().Direction == TradeDirection.Sell ? Trades.Last().VolumeEur : InitialInvestment;

                        Trades.Add(new Trade()
                        {
                            TradeDate = candle.CloseDate,
                            Direction = TradeDirection.Buy,
                            Price = candle.Close,
                            Volume = Trades.Any() ? Trades.Last().VolumeEur / candle.Close : InitialInvestment / candle.Close,
                            VolumeEur = Trades.Any() ? Trades.Last().VolumeEur : InitialInvestment
                        });
                    }

                    //TODO: Add sell signal object with perc
                    //TODO: support 2 types of api, one with delegate and other with expression
                    if (sell.Invoke(candles, i) && 
                        await _shouldSell.Invoke(Trades, candle))
                    {
                        //How to get sell amount from user?
                        var v = Trades.Any() && Trades.Last().Direction == TradeDirection.Buy ? Trades.Last().Volume : InitialInvestment;
                        Trades.Add(new Trade()
                        {
                            TradeDate = candle.CloseDate,
                            Direction = TradeDirection.Sell,
                            Price = candle.Close,
                            Volume = v,
                            VolumeEur = v * candle.Close,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    throw;
                }
            }

            if (Trades.Any())
            {
                ProfitEur = Trades.Last().Direction == TradeDirection.Sell ? Trades.Last().VolumeEur : Trades.Last().Volume * candles[candles.Count - 1].Close;
                ProfitCoin = Trades.Last().Direction == TradeDirection.Buy ? Trades.Last().Volume : Trades.Last().VolumeEur / candles[candles.Count - 1].Close;
            }
        }
    }
}
