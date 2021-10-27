using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.IndexPrices.Client;
using Service.Liquidity.DwhDataJob.Postgres;
using Service.Liquidity.DwhDataJob.Postgres.Models;

namespace Service.Liquidity.DwhDataJob.Engines
{
    public class ConvertPriceEngine
    {
        private readonly ILogger<ConvertPriceEngine> _logger;
        private readonly IConvertIndexPricesClient _convertIndexPricesClient;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public ConvertPriceEngine(IConvertIndexPricesClient convertIndexPricesClient, 
            ILogger<ConvertPriceEngine> logger, 
            DatabaseContextFactory databaseContextFactory)
        {
            _convertIndexPricesClient = convertIndexPricesClient;
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task HandleConvertPrice()
        {
            var prices = _convertIndexPricesClient.GetConvertIndexPricesAsync();
            var pricesEntities = prices.Select(e => new ConvertIndexPriceEntity(e)).ToList();

            await using var ctx = _databaseContextFactory.Create();
            await ctx.UpsertConvertPrice(pricesEntities);
            _logger.LogInformation($"ConvertPriceEngine.HandleConvertPrice saved {pricesEntities.Count} entities.");
        }
    }
}