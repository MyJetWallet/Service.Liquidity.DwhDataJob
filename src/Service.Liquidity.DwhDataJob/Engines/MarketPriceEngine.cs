using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.IndexPrices.Client;
using Service.Liquidity.DwhDataJob.Postgres;
using Service.Liquidity.DwhDataJob.Postgres.Models;

namespace Service.Liquidity.DwhDataJob.Engines
{
    public class MarketPriceEngine
    {
        private readonly ILogger<MarketPriceEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly ICurrentPricesClient _currentPricesClient;

        public MarketPriceEngine(DatabaseContextFactory databaseContextFactory, 
            ILogger<MarketPriceEngine> logger, 
            ICurrentPricesClient currentPricesClient)
        {
            _databaseContextFactory = databaseContextFactory;
            _logger = logger;
            _currentPricesClient = currentPricesClient;
        }
        
        public async Task HandleMarketPrice()
        {
            var prices = _currentPricesClient.GetAllPrices();
            var pricesEntities = prices.Select(e => new MarketPriceEntity(e)).ToList();

            await using var ctx = _databaseContextFactory.Create();
            await ctx.UpsertMarketPrice(pricesEntities);
            _logger.LogInformation($"MarketPriceEngine.HandleMarketPrice saved {pricesEntities.Count} entities.");
        }
    }
}