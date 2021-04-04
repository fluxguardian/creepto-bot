using Microsoft.Extensions.Logging;
using StrategyTester.Enums;
using StrategyTester.Exchanges;
using StrategyTester.Technical_Analysis;
using StrategyTester.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTester.Runners
{
    internal class SignalRunner
    {
        private readonly ILogger<SignalRunner> _logger;
        private readonly BinanceApi _binanceApi;
        private readonly StrategyFactory _strategyFactory;
        private readonly TelegramApi _telegram;
        private readonly IDictionary<long, Candle> _candles;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Strategy _strategy;

        public SignalRunner(ILogger<SignalRunner> logger, BinanceApi binanceApi, StrategyFactory strategyFactory, TelegramApi telegram)
        {
            _logger = logger;
            _binanceApi = binanceApi;
            _strategyFactory = strategyFactory;
            _telegram = telegram;
            _candles = new Dictionary<long, Candle>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal decimal Profit
            => _strategy.Trades.Any(t => t.Direction == TradeDirection.Sell) ? _strategy.Trades.Last(t => t.Direction == TradeDirection.Sell).VolumeEur : 0;

        internal IList<Trade> Trades
            => _strategy.Trades;

        internal void Stop()
            => _cancellationTokenSource.Cancel();

        internal void Start(StrategyContext strategyContext)
        {
            _strategy = _strategyFactory.GetStrategy(
                strategyContext.StrategyName,
                async (trades, candle) =>
                {
                    //TODO: move messages to on sell event
                    await _telegram.SendMessageAsync($"{strategyContext.Market.ToUpperInvariant()} Buy signal @ {candle.Close} Will buy? {!trades.Any() || trades[trades.Count - 1].Direction == TradeDirection.Sell}");
                    return !trades.Any() || trades[trades.Count - 1].Direction == TradeDirection.Sell;
                },
                async (trades, candle) =>
                {
                    await _telegram.SendMessageAsync($"{strategyContext.Market.ToUpperInvariant()} Sell Signal @ {candle.Close} Will sell? {trades.Any() && trades[trades.Count - 1].Direction == TradeDirection.Buy}");
                    return trades.Any() && trades[trades.Count - 1].Direction == TradeDirection.Buy;
                });

            var cancellationToken = _cancellationTokenSource.Token;
            Task.Run(() => ExecuteStrategy(strategyContext, cancellationToken), cancellationToken);

            // For now I dont use websockts because too fast updates, maybe I should only take into consideration closed candles for signals?
            // Maybe add 2 modes and test how they perform in the market

            //candles.ForEach(c => _candles.Add(c.OpenTime, c));

            //var subscription = new BinanceKLineSubscription()
            //{
            //    Symbol = strategyContext.Market.ToLowerInvariant(),
            //    Interval = "1h"
            //};
            //await _binanceApi.ConnectToWebSockets(subscription, OnCandleUpdate, cancellationToken);
        }

        private async Task ExecuteStrategy(StrategyContext strategyContext, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _logger.LogInformation("Getting candles...");

                var candles = (await _binanceApi.GetCandles(strategyContext.Market, strategyContext.Interval, 25)).ToList();

                await _strategy.Execute(candles);

                await Task.Delay(TimeSpan.FromMinutes(30), token);
            }
        }

        private async void OnCandleUpdate(object sender, BinanceKLineEventArgs e)
        {
            _logger.LogInformation("Got new candle...");

            var kLine = e.KLine.KLine;
            var candle = new Candle()
            {
                Close = kLine.ClosePrice,
                CloseTime = kLine.CloseTime,
                OpenTime = kLine.StartTime,
                Open = kLine.OpenPrice,
                High = kLine.HighPrice,
                Low = kLine.LowPrice,
                NumberOfTrades = kLine.NumberOfTrades,
                QuoteAssetVolume = kLine.QuoteAssetVolume,
            };

            if (!_candles.ContainsKey(kLine.StartTime))
            {
                _candles.Add(kLine.StartTime, candle);
                _candles.Remove(_candles.Keys.First());
            }
            else
            {
                _candles[kLine.StartTime] = candle;
            }

            await _strategy.Execute(_candles.Values.ToList());

            _logger.LogInformation("Strategy executed...");
        }
    }
}