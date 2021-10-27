using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Liquidity.DwhDataJob.Engines;

namespace Service.Liquidity.DwhDataJob.Jobs
{
    public class IndexPriceJob : IStartable
    {
        private readonly ILogger<IndexPriceJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly MarketPriceEngine _marketPriceEngine;
        private readonly ConvertPriceEngine _convertPriceEngine;

        public IndexPriceJob(ILogger<IndexPriceJob> logger,
            MarketPriceEngine marketPriceEngine, 
            ConvertPriceEngine convertPriceEngine)
        {
            _logger = logger;
            _marketPriceEngine = marketPriceEngine;
            _convertPriceEngine = convertPriceEngine;

            _timer = new MyTaskTimer(nameof(IndexPriceJob), TimeSpan.FromSeconds(Program.Settings.MarketPriceJobTimerInSeconds), _logger, DoTime);
            Console.WriteLine($"MarketPriceJob timer: {TimeSpan.FromSeconds(Program.Settings.MarketPriceJobTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            _logger.LogInformation($"MarketPriceJob started at {DateTime.UtcNow}");
            await _marketPriceEngine.HandleMarketPrice();
            await _convertPriceEngine.HandleConvertPrice();
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}