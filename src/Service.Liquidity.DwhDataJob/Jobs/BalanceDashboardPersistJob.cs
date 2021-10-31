using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Liquidity.DwhDataJob.Engines;
using Service.Liquidity.DwhDataJob.Postgres;

namespace Service.Liquidity.DwhDataJob.Jobs
{
    public class BalanceDashboardPersistJob : IStartable
    {
        private readonly ILogger<BalanceDashboardPersistJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly BalanceDashboardEngine _balanceDashboardEngine;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public BalanceDashboardPersistJob(ILogger<BalanceDashboardPersistJob> logger, 
            BalanceDashboardEngine balanceDashboardEngine, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _balanceDashboardEngine = balanceDashboardEngine;
            _databaseContextFactory = databaseContextFactory;

            _timer = new MyTaskTimer(nameof(BalanceDashboardPersistJob), 
                TimeSpan.FromSeconds(Program.Settings.BalanceDashboardPersistJobTimerInSeconds), _logger, DoTime);
            Console.WriteLine($"BalanceDashboardPersistJob timer: {TimeSpan.FromSeconds(Program.Settings.BalanceDashboardPersistJobTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            await PersistDashboard();
        }

        private async Task PersistDashboard()
        {
            var todaySnapshot = _balanceDashboardEngine.GetTodayDashboardSnapshot();
            _logger.LogInformation($"BalanceDashboardPersistJob persist {todaySnapshot.Count} records at {DateTime.UtcNow}");
            await using var ctx = _databaseContextFactory.Create();
            await ctx.UpsertBalanceDashboard(todaySnapshot);
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}