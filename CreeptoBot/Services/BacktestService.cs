using Microsoft.Extensions.Hosting;
using StrategyTester.Enums;
using StrategyTester.Exchanges;
using StrategyTester.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTester.Services
{
    public class BacktestService : BackgroundService
    {
        private readonly BinanceApi _api;
        private readonly TelegramApi _telegram;
        private readonly StrategyFactory _strategyFactory;

        public BacktestService(BinanceApi api, TelegramApi telegram, StrategyFactory strategyFactory)
        {
            _api = api;
            _telegram = telegram;
            _strategyFactory = strategyFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _telegram.OnMessageReceived += OnTelegramMessage;
            _telegram.StartReceivingMessages(cancellationToken);
        }

        private async Task RunTestAsync(string market, string candleSize, string strategyName)
        {
            var candles = (await _api.GetCandles(market, candleSize, 1000)).ToList();

            var strategy = _strategyFactory.GetStrategy(strategyName);

            await strategy.Execute(candles);

            var sb = new StringBuilder();

            if (strategy.Trades.Any())
            {
                decimal totalProfit = strategy.Trades.Last(t => t.Direction == TradeDirection.Sell).VolumeEur;
                sb.AppendLine($"Start-End Date:{candles.First().OpenDate} - {candles.Last().OpenDate} {(candles.First().OpenDate - candles.Last().OpenDate).Days} days");
                sb.AppendLine($"InitialInvestment: {strategy.InitialInvestment}");
                sb.AppendLine($"Pnl: {totalProfit.Round(2)} ({MathExtensions.PercentageDifference(strategy.InitialInvestment, totalProfit)}%)");
                sb.AppendLine($"Trades: ");
                for (int i = 0; i < strategy.Trades.Count; i++)
                {
                    var p = strategy.Trades[i];
                    if (p.Direction == TradeDirection.Buy)
                    {
                        var percentage = i > 0 ? MathExtensions.PercentageDifference(strategy.Trades[i - 1].Volume, p.Volume) : 0;
                        sb.AppendLine($"{p.TradeDate}: {p.Direction} >> {Math.Round(p.VolumeEur, 2)}EUR * {p.Price.Round(2)} = {p.Volume.Round(2)}??? ({percentage.Round(2)}%)");
                    }
                    if (p.Direction == TradeDirection.Sell)
                    {
                        var percentage = i > 0 ? MathExtensions.PercentageDifference(strategy.Trades[i - 1].VolumeEur, p.VolumeEur) : 0;

                        sb.AppendLine($"{p.TradeDate}: {p.Direction} >> {strategy.Trades[i - 1].Volume.Round(2)}??? @ {p.Price.Round(2)} = {Math.Round(p.VolumeEur, 2)}EUR ({percentage.Round(2)}%)");
                    }
                }
            }
            else
            {
                sb.AppendLine("No trades executed");
            }

            await _telegram.SendMessageAsync(sb.ToString());
        }

        private const string RUN_MESSAGE = "Run";
        private async void OnTelegramMessage(object sender, MessageReceivedEventArgs e)
        {
            var args = e.Message.Text.Split(' ');
            if (args.Length < 3)
            {
                throw new ArgumentException("Invalid number of arguments. usage: RUN <market_name> <candle_size> <strategy_name>");
            }
            switch (args[0])
            {
                case RUN_MESSAGE:
                    await _telegram.SendMessageAsync("Starting test run...");
                    var market = args[1];
                    var candleSize = args[2];
                    var strategyName = args[3];

                    await RunTestAsync(market, candleSize, strategyName);

                    break;
            }
        }
    }
}
