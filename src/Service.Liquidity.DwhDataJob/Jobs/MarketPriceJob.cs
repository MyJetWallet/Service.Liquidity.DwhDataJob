using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Liquidity.DwhDataJob.Engines;

namespace Service.Liquidity.DwhDataJob.Jobs
{
    public class MarketPriceJob : IStartable
    {
        private readonly ILogger<MarketPriceJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly MarketPriceEngine _marketPriceEngine;

        public MarketPriceJob(ILogger<MarketPriceJob> logger,
            MarketPriceEngine marketPriceEngine)
        {
            _logger = logger;
            _marketPriceEngine = marketPriceEngine;

            _timer = new MyTaskTimer(nameof(MarketPriceJob), TimeSpan.FromSeconds(Program.Settings.MarketPriceJobTimerInSeconds), _logger, DoTime);
            Console.WriteLine($"MarketPriceJob timer: {TimeSpan.FromSeconds(Program.Settings.MarketPriceJobTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            _logger.LogInformation($"MarketPriceJob started at {DateTime.UtcNow}");
            await _marketPriceEngine.HandleMarketPrice();
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}