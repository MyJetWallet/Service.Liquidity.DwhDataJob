using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ME.Contracts.OutgoingMessages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.ClientWallets.Domain.Models;
using Service.ClientWallets.Grpc;
using Service.ClientWallets.Grpc.Models;
using Service.Liquidity.DwhDataJob.Domain.Models;
using Service.Liquidity.DwhDataJob.Postgres;

namespace Service.Liquidity.DwhDataJob.Engines
{
    public class BalanceDashboardEngine : IStartable
    {
        private readonly ILogger<BalanceDashboardEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IClientWalletService _clientWalletService;
        private List<BalanceDashboard> DashboardList { get; set; }
        private readonly object _locker = new object();
        private static DateTime Today =>  DateTime.UtcNow.Date;

        public BalanceDashboardEngine(ILogger<BalanceDashboardEngine> logger, 
            DatabaseContextFactory databaseContextFactory, 
            IClientWalletService clientWalletService)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _clientWalletService = clientWalletService;
        }

        public List<BalanceDashboard> GetTodayDashboardSnapshot()
        {
            List<BalanceDashboard> todayDashboardSnapshot;
            lock (_locker)
            {
                todayDashboardSnapshot =
                    DashboardList
                        .Where(e => e.BalanceDate == Today)
                        .Select(BalanceDashboard.GetCopy)
                        .ToList();
            }
            return todayDashboardSnapshot;
        }
        
        public async Task UpdateDashboard(BalanceUpdate update, long headerSequenceNumber)
        {
            var walletInfo = await _clientWalletService.GetWalletInfoByIdAsync(new GetWalletInfoByIdRequest()
            {
                WalletId = update.WalletId
            });
            await UpdateBalance(walletInfo.WalletInfo, update, headerSequenceNumber);
        }

        private async Task UpdateBalance(ClientWallet walletInfo, BalanceUpdate update, long headerSequenceNumber)
        {
            var retryCount = 0;
            while (!DashboardList.Any())
            {
                _logger.LogInformation($"DashboardList is empty. UpdateBalance retry iteration {retryCount}.");
                if (retryCount >= 300)
                {
                    break;
                }
                retryCount++;
                await Task.Delay(1000);
            }

            if (!DashboardList.Any())
            {
                _logger.LogError("DashboardList is empty. Skip message: {messageJson}", JsonConvert.SerializeObject(update));
                return;
            }
            
            lock (_locker)
            {
                _logger.LogInformation("Start update balance for date = {today}, asset = {asset} and walletId = {walletId}",
                    Today, update.AssetId, update.WalletId);
                
                var todayBalances = DashboardList
                    .Where(e => e.BalanceDate == Today)
                    .ToList();
                if (todayBalances.Any())
                {
                    UpdateTodayBalance(walletInfo, todayBalances, update, headerSequenceNumber);
                }
                else
                {
                    CreateAndUpdateTodayBalance(walletInfo, update, headerSequenceNumber);
                }
            }
        }

        private void CreateAndUpdateTodayBalance(ClientWallet walletInfo, BalanceUpdate update,
            long headerSequenceNumber)
        {
            var todayBalances = CreateTodayBalances(update);
            UpdateTodayBalance(walletInfo, todayBalances, update, headerSequenceNumber);
            DashboardList.AddRange(todayBalances);
        }

        private List<BalanceDashboard> CreateTodayBalances(BalanceUpdate update)
        {
            var todayBalances = new List<BalanceDashboard>();
            foreach (var balanceDashboard in DashboardList.OrderByDescending(e => e.BalanceDate))
            {
                var todayBalanceByAsset =
                    todayBalances.FirstOrDefault(e => e.Asset == balanceDashboard.Asset);
                if (todayBalanceByAsset != null)
                    continue;

                var lastBalance = DashboardList
                    .Where(e => e.Asset == balanceDashboard.Asset)
                    .OrderByDescending(e => e.BalanceDate)
                    .FirstOrDefault();
                if (lastBalance == null)
                {
                    _logger.LogError($"Cannot find balance for asset: {balanceDashboard.Asset}. " +
                                     $"Skip update: {JsonConvert.SerializeObject(update)}");
                    continue;
                }
                todayBalances.Add(new BalanceDashboard()
                {
                    BalanceDate = Today,
                    LastUpdateDate = lastBalance.LastUpdateDate,
                    Asset = balanceDashboard.Asset,
                    ClientBalance = lastBalance.ClientBalance,
                    BrokerBalance = lastBalance.BrokerBalance,
                    Commission = lastBalance.Commission,
                    LastMessageId = lastBalance.LastMessageId
                });
            }
            return todayBalances;
        }

        private void UpdateTodayBalance(ClientWallet walletInfo,
            IEnumerable<BalanceDashboard> dashboardList, BalanceUpdate update, long headerSequenceNumber)
        {
            var balanceByAsset = dashboardList.FirstOrDefault(e => e.Asset == update.AssetId);
            if (balanceByAsset == null)
            {
                var message = $"Cannot find balance for date: {Today} and asset: {update.AssetId}. Skip update: {JsonConvert.SerializeObject(update)}";
                _logger.LogError(message);
                return;
            }
            var difference = Convert.ToDecimal(update.NewBalance) - Convert.ToDecimal(update.OldBalance);

            if (walletInfo.IsInternal)
            {
                balanceByAsset.BrokerBalance += difference;
            }
            else
            {
                balanceByAsset.ClientBalance += difference;
            }
            balanceByAsset.LastUpdateDate = DateTime.UtcNow;
            balanceByAsset.LastMessageId = headerSequenceNumber;
        }

        public void Start()
        {
            LoadDashboardList().GetAwaiter().GetResult();
        }

        private async Task LoadDashboardList()
        {
            await using var ctx = _databaseContextFactory.Create();
            var dashboardList = ctx.GetBalanceDashboardList(DateTime.UtcNow.Date);
            _logger.LogInformation($"LoadDashboardList find {dashboardList.Count} records.");
            if (!dashboardList.Any())
            {
                _logger.LogInformation("LoadDashboardList cannot find records. Migration started.");
                await ctx.ExecBalanceDashboardMigrationAsync(_logger);
                dashboardList = ctx.GetBalanceDashboardList(DateTime.UtcNow.Date);
                _logger.LogInformation($"LoadDashboardList find {dashboardList.Count} records after migration.");
            }
            lock (_locker)
            {
                DashboardList = dashboardList ?? new List<BalanceDashboard>();
            }
        }
    }
}