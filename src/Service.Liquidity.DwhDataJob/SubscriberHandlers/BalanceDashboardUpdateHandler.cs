using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using ME.Contracts.OutgoingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Liquidity.DwhDataJob.Engines;

namespace Service.Liquidity.DwhDataJob.SubscriberHandlers
{
    public class BalanceDashboardUpdateHandler : IStartable
    {
        private readonly ILogger<BalanceDashboardUpdateHandler> _logger;
        private readonly BalanceDashboardEngine _balanceDashboardEngine;

        public BalanceDashboardUpdateHandler(ILogger<BalanceDashboardUpdateHandler> logger,
            ISubscriber<IReadOnlyList<OutgoingEvent>> subscriber, 
            BalanceDashboardEngine balanceDashboardEngine)
        {
            _logger = logger;
            _balanceDashboardEngine = balanceDashboardEngine;

            subscriber.Subscribe(HandleEvents);
        }

        private async ValueTask HandleEvents(IReadOnlyList<OutgoingEvent> events)
        {
            using var activity = MyTelemetry.StartActivity("Handle ME OutgoingEvents")?.AddTag("count-events", events.Count);
            try
            {
                foreach (var meEvent in events)
                {
                    foreach (var update in meEvent.BalanceUpdates)
                    {
                        await _balanceDashboardEngine.UpdateDashboard(update);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                events.AddToActivityAsJsonTag("me events");
                throw;
            }
        }

        public void Start()
        {
        }
    }
}