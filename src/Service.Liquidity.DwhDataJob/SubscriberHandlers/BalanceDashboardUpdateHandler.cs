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

                    var hasDuplicate = new Dictionary<string, bool>();
                    
                    foreach (var asset in meEvent.BalanceUpdates.Select(e => e.AssetId).Distinct())
                    {
                        var dashboardSnapshot = _balanceDashboardEngine.GetTodayDashboardSnapshot();
                        var lastBalanceByAsset = dashboardSnapshot
                            .Where(e => e.Asset == asset)
                            .OrderByDescending(e => e.LastUpdateDate)
                            .Take(1)
                            .FirstOrDefault();
                        bool isDouble;
                        if (lastBalanceByAsset == null ||
                            lastBalanceByAsset.LastMessageId != messageId)
                        {
                            isDouble = false;
                        }
                        else
                        {
                            isDouble = true;
                            _logger.LogError($"BalanceDashboardUpdateHandler handle duplicate message. SequenceNumber = {messageId}");
                        }
                        hasDuplicate.Add(asset, isDouble);
                    }
                    
                    foreach (var update in meEvent.BalanceUpdates)
                    {
                        if (!hasDuplicate[update.AssetId])
                        {
                            await _balanceDashboardEngine.UpdateDashboard(update, messageId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                events.AddToActivityAsJsonTag("me events");
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public void Start()
        {
        }
    }
}