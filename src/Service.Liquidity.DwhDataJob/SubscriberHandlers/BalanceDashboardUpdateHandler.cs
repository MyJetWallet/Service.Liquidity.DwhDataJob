using System;
using System.Collections.Generic;
using System.Linq;
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
                    var messageId = meEvent.Header.SequenceNumber;
                    foreach (var update in meEvent.BalanceUpdates)
                    {
                        var dashboardSnapshot = _balanceDashboardEngine.GetTodayDashboardSnapshot();
                        var lastBalanceByAsset = dashboardSnapshot
                            .Where(e => e.Asset == update.AssetId)
                            .OrderByDescending(e => e.LastUpdateDate)
                            .FirstOrDefault();

                        if (lastBalanceByAsset == null ||
                            lastBalanceByAsset.LastMessageId != messageId)
                        {
                            await _balanceDashboardEngine.UpdateDashboard(update, messageId);
                        }
                        else
                        {
                            _logger.LogError($"BalanceDashboardUpdateHandler handle duplicate message. SequenceNumber = {messageId}");
                        }
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