using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Liquidity.DwhDataJob.Engines;
using Service.Liquidity.DwhDataJob.Postgres;

namespace Service.Liquidity.DwhDataJob.Jobs
{
    public class CommissionDashboardPersistJob : IStartable
    {
        private readonly ILogger<CommissionDashboardPersistJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly CommissionDashboardEngine _commissionDashboardEngine;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public CommissionDashboardPersistJob(ILogger<CommissionDashboardPersistJob> logger, 
            CommissionDashboardEngine commissionDashboardEngine, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _commissionDashboardEngine = commissionDashboardEngine;
            _databaseContextFactory = databaseContextFactory;

            _timer = new MyTaskTimer(nameof(CommissionDashboardPersistJob), 
                TimeSpan.FromSeconds(Program.Settings.CommissionDashboardPersistJobTimerInSeconds), _logger, DoTime);
            Console.WriteLine($"CommissionDashboardPersistJob timer: {TimeSpan.FromSeconds(Program.Settings.CommissionDashboardPersistJobTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            await PersistDashboard();
        }

        private async Task PersistDashboard()
        {
            var todaySnapshot = _commissionDashboardEngine.GetTodayDashboardSnapshot();
            _logger.LogInformation($"CommissionDashboardPersistJob persist {todaySnapshot.Count} records at {DateTime.UtcNow}");
            await using var ctx = _databaseContextFactory.Create();
            await ctx.UpsertCommissionDashboard(todaySnapshot);
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}