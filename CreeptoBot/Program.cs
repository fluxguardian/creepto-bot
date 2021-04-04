using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StrategyTester.Exchanges;
using StrategyTester.Runners;
using StrategyTester.Services;
using System;
using System.Threading.Tasks;

namespace StrategyTester
{
    public class Program
    {
        public static async Task Main(string[] args)
            => await CreateHostBuilder().Build().RunAsync();

        public static IHostBuilder CreateHostBuilder()
            => new HostBuilder()
                .UseSerilog(BuildLogger())
                .ConfigureServices((h, sc) =>
                {
                    sc.AddSingleton<BinanceApi>();
                    sc.AddSingleton<TelegramApi>();
                    sc.AddSingleton<StrategyFactory>();
                    sc.AddHostedService<SignalsService>();
                    sc.AddHostedService<BacktestService>();
                    sc.AddTransient<SignalRunner>();
                    sc.Configure<TelegramApiSettings>(h.Configuration.GetSection(TelegramApiSettings.Section));
                    sc.Configure<BinanceSettings>(h.Configuration.GetSection(BinanceSettings.Section));
                })
                .ConfigureAppConfiguration((h, cb) =>
                {
                    var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                    cb.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
                    cb.AddJsonFile($"appSettings.{environmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                    loggingBuilder.AddSerilog();
                });

        private static ILogger BuildLogger()
            => new LoggerConfiguration()
                        .WriteTo.Console()
                        .MinimumLevel.Debug()
                        .CreateLogger();
    }
}
