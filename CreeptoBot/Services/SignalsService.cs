using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StrategyTester.Enums;
using StrategyTester.Runners;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTester.Services
{
    public class SignalsService : BackgroundService
    {
        private readonly IDictionary<string, SignalRunner> _signalWatches;
        private readonly TelegramApi _telegram;
        private readonly ILogger<SignalsService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SignalsService(TelegramApi telegram, ILogger<SignalsService> logger, IServiceProvider serviceProvider)
        {
            _telegram = telegram;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _signalWatches = new ConcurrentDictionary<string, SignalRunner>();
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("Signals Service Executing..");
            _telegram.StartReceivingMessages(token, Telegram_OnMessageReceived);

            await _telegram.SendMessageAsync("Creepto signals bot started..");
        }

        public const string PNL_MESSAGE = "pnl";
        public const string TRADES_MESSAGE = "trades";
        public const string REMOVE_WATCH_MESSAGE = "removewatch";
        public const string ADD_WATCH_MESSAGE = "addwatch";
        public const string LIST_WATCHES_MESSAGE = "listwatches";
        public const string PING_MESSAGE = "ping";

        private async void Telegram_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var args = e.Message.Text.Split(' ');

            switch (args[1])
            {
                case PNL_MESSAGE: //signals pnl adaeur 1h maup
                    if (args.Length != 5)
                    {
                        await _telegram.SendMessageAsync($"PNL :: Invalid Number Of Arguments");
                        return;
                    }

                    if (_signalWatches.TryGetValue(string.Concat(args[2], args[3], args[4]), out var runner))
                    {
                        await _telegram.SendMessageAsync($" Trades: {runner.Trades.Count()} Profit: {runner.Profit}");
                    }

                    break;
                case TRADES_MESSAGE: //signals trades adaeur 1h maup
                    if (args.Length != 5)
                    {
                        await _telegram.SendMessageAsync($"PNL :: Invalid Number Of Arguments");
                        return;
                    }

                    if (_signalWatches.TryGetValue(string.Concat(args[2], args[3], args[4]), out var runner2))
                    {
                        for (int i = 0; i < runner2.Trades.Count; i++)
                        {
                            var p = runner2.Trades[i];
                            if (p.Direction == TradeDirection.Buy)
                            {
                                await _telegram.SendMessageAsync($"Type: {p.Direction} >> {p.Volume}??? @ {p.Price} = {Math.Round(p.VolumeEur, 2)}");
                            }
                            if (p.Direction == TradeDirection.Sell)
                            {
                                var percentage = (p.VolumeEur - runner2.Trades[i - 1].VolumeEur) / runner2.Trades[i - 1].VolumeEur * 100;
                                await _telegram.SendMessageAsync($"Type: {p.Direction} >> {runner2.Trades[i - 1].Volume}??? @ {p.Price} = {Math.Round(p.VolumeEur, 2)}EUR ({percentage}%)");
                            }
                        }
                    }
                    break;
                case REMOVE_WATCH_MESSAGE:
                    if (args.Length != 5)
                    {
                        await _telegram.SendMessageAsync($"REMOVE WATCH :: Invalid Number Of Arguments");
                        return;
                    }

                    var key = string.Concat(args[2], args[3], args[4]);

                    if (_signalWatches.TryGetValue(key, out var runner3))
                    {
                        _signalWatches.Remove(key);
                        runner3.Stop();
                    }

                    await _telegram.SendMessageAsync($"REMOVE WATCH :: Command Successful");
                    break;
                case ADD_WATCH_MESSAGE:
                    if (args.Length != 5)
                    {
                        await _telegram.SendMessageAsync($"ADD WATCH :: Invalid Number Of Arguments");
                        return;
                    }

                    SignalRunner signalRunner = (SignalRunner)_serviceProvider.GetService(typeof(SignalRunner));
                    var strategyContext = new StrategyContext(args[2], args[3], args[4]);

                    signalRunner.Start(strategyContext);

                    _signalWatches.Add($"{string.Concat(args[2], args[3], args[4]).ToLowerInvariant()}", signalRunner);

                    await _telegram.SendMessageAsync($"ADD WATCH :: Command Successful");
                    break;
                case LIST_WATCHES_MESSAGE: // signals listwatches
                    if (args.Length != 5)
                    {
                        await _telegram.SendMessageAsync($"LIST WATCHES :: Invalid Number Of Arguments");
                        return;
                    }
                    var sb = new StringBuilder();

                    _signalWatches.Keys.ToList().ForEach(s => sb.AppendLine(s));

                    await _telegram.SendMessageAsync(sb.ToString());
                    break;
                case PING_MESSAGE:
                    await _telegram.SendMessageAsync($"Pong");
                    break;
            }
        }
    }
}
