using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Newtonsoft.Json;
using Service.Liquidity.DwhDataJob.Postgres;
using Service.Liquidity.DwhDataJob.Postgres.Models;
using Service.Liquidity.InternalWallets.Grpc;
using Service.Liquidity.InternalWallets.Grpc.Models;

namespace Service.Liquidity.DwhDataJob.Jobs
{
    public class ExternalBalancesJob : IStartable
    {
        private readonly ILogger<ExternalBalancesJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly IExternalMarketsGrpc _externalMarketsGrpc;
        private readonly DatabaseContextFactory _databaseContextFactory;
    
        public ExternalBalancesJob(ILogger<ExternalBalancesJob> logger, 
            IExternalMarketsGrpc externalMarketsGrpc, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _externalMarketsGrpc = externalMarketsGrpc;
            _databaseContextFactory = databaseContextFactory;

            _timer = new MyTaskTimer(nameof(ExternalBalancesJob), 
                TimeSpan.FromSeconds(Program.Settings.ExternalBalancesJobTimerInSeconds), _logger, DoTime);
            Console.WriteLine($"ExternalBalancesJob timer: {TimeSpan.FromSeconds(Program.Settings.ExternalBalancesJobTimerInSeconds)}");
        }
    
        private async Task DoTime()
        {
            await PersistExternalBalances();
        }

        private async Task PersistExternalBalances()
        {
            try
            {
                _logger.LogInformation($"PersistExternalBalances start at {DateTime.UtcNow}");
                
                var externalExchanges = await _externalMarketsGrpc.GetExternalMarketListAsync();
                var exchangesList = externalExchanges.Data.List;
                
                _logger.LogInformation("PersistExternalBalances find exchanges: {exchangesJson}.", 
                    JsonConvert.SerializeObject(exchangesList));
                
                var allBalances = new List<ExternalBalanceEntity>();
                foreach (var exchange in exchangesList)
                {
                    var balances = await _externalMarketsGrpc.GetBalancesAsync(new SourceDto()
                    {
                        Source = exchange
                    });
                    _logger.LogInformation("PersistExternalBalances find {balanceCount} for exchange: {exchangeJson}.", 
                        balances.Data.List.Count, exchange);
                    
                    allBalances.AddRange(balances.Data.List.Select(e => new ExternalBalanceEntity()
                    {
                        Exchange = exchange,
                        Asset = e.Asset,
                        Balance = (decimal) e.Balance,
                        BalanceDate = DateTime.UtcNow.Date,
                        LastUpdateDate = DateTime.UtcNow
                    }));
                }
                await using var ctx = _databaseContextFactory.Create();
                await ctx.UpsertExternalBalances(allBalances);
                _logger.LogInformation("PersistExternalBalances saved {balanceCount} balances.", 
                    allBalances.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}