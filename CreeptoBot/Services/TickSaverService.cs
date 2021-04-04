using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTester.Services
{
    internal class CandlePercistenceService : BackgroundService
    {
        public CandlePercistenceService()
        {

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
