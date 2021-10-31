using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.DwhDataJob.Domain.Models;
using Service.Liquidity.DwhDataJob.Postgres;

namespace Service.Liquidity.DwhDataJob.Engines
{
    public class CommissionDashboardEngine : IStartable
    {
        private readonly ILogger<CommissionDashboardEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        
        private List<CommissionDashboard> DashboardList { get; set; }
        private readonly object _locker = new object();
        private static DateTime Today =>  DateTime.UtcNow.Date;

        public CommissionDashboardEngine(ILogger<CommissionDashboardEngine> logger, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
        }
        
        public List<CommissionDashboard> GetTodayDashboardSnapshot()
        {
            List<CommissionDashboard> todayDashboardSnapshot;
            lock (_locker)
            {
                todayDashboardSnapshot =
                    DashboardList
                        .Where(e => e.CommissionDate == Today)
                        .Select(CommissionDashboard.GetCopy)
                        .ToList();
            }
            return todayDashboardSnapshot;
        }
        
        public async Task UpdateDashboard(SwapMessage message, string id)
        {
            lock (_locker)
            {
                _logger.LogInformation("Start update commission for date = {today}, asset = {asset}.",
                    Today, message.DifferenceAsset);
                
                var todayBalances = DashboardList
                    .Where(e => e.CommissionDate == Today)
                    .ToList();
                if (todayBalances.Any())
                {
                    UpdateTodayBalance(todayBalances, message, id);
                }
                else
                {
                    CreateAndUpdateTodayBalance(message, id);
                }
            }
        }
        
        private void CreateAndUpdateTodayBalance(SwapMessage message, string id)
        {
            var todayBalances = CreateTodayBalances(message);
            UpdateTodayBalance(todayBalances, message, id);
            DashboardList.AddRange(todayBalances);
        }
        
        private List<CommissionDashboard> CreateTodayBalances(SwapMessage message)
        {
            var todayBalances = new List<CommissionDashboard>();
            foreach (var dashboard in DashboardList.OrderByDescending(e => e.CommissionDate))
            {
                var todayBalanceByAsset =
                    todayBalances.FirstOrDefault(e => e.Asset == dashboard.Asset);
                if (todayBalanceByAsset != null)
                    continue;

                var lastCommission = DashboardList
                    .Where(e => e.Asset == dashboard.Asset)
                    .OrderByDescending(e => e.CommissionDate)
                    .FirstOrDefault();
                if (lastCommission == null)
                {
                    _logger.LogError($"Cannot find commission for asset: {dashboard.Asset}. " +
                                     $"Skip update: {JsonConvert.SerializeObject(message)}");
                    continue;
                }
                todayBalances.Add(new CommissionDashboard()
                {
                    CommissionDate = Today,
                    LastUpdateDate = lastCommission.LastUpdateDate,
                    Asset = dashboard.Asset,
                    Commission = lastCommission.Commission,
                    LastMessageId = lastCommission.LastMessageId
                });
            }
            return todayBalances;
        }
        
        private void UpdateTodayBalance(List<CommissionDashboard> dashboardList, SwapMessage swap, string id)
        {
            var commissionByAsset = dashboardList.FirstOrDefault(e => e.Asset == swap.DifferenceAsset);
            if (commissionByAsset == null)
            {
                var newCommission = new CommissionDashboard()
                {
                    Asset = swap.DifferenceAsset,
                    CommissionDate = Today
                };
                DashboardList.Add(newCommission);
                commissionByAsset = newCommission;
            }
            commissionByAsset.Commission += swap.DifferenceVolumeAbs;
            commissionByAsset.LastUpdateDate = DateTime.UtcNow;
            commissionByAsset.LastMessageId = id;
        }

        public void Start()
        {
            LoadDashboardList().GetAwaiter().GetResult();
        }
        
        private async Task LoadDashboardList()
        {
            await using var ctx = _databaseContextFactory.Create();
            var dashboardList = ctx.GetCommissionDashboardList(DateTime.UtcNow.Date);
            _logger.LogInformation($"LoadDashboardList find {dashboardList.Count} records.");
            lock (_locker)
            {
                DashboardList = dashboardList ?? new List<CommissionDashboard>();
            }
        }
    }
}