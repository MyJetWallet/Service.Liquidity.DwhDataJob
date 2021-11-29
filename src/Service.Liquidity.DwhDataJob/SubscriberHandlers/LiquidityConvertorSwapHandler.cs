using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.DwhDataJob.Engines;

namespace Service.Liquidity.DwhDataJob.SubscriberHandlers
{
    public class LiquidityConvertorSwapHandler : IStartable
    {
        private readonly ILogger<LiquidityConvertorSwapHandler> _logger;
        private readonly CommissionDashboardEngine _commissionDashboardEngine;
        
        public LiquidityConvertorSwapHandler(ISubscriber<SwapMessage> swapSubscriber, 
            ILogger<LiquidityConvertorSwapHandler> logger, 
            CommissionDashboardEngine commissionDashboardEngine)
        {
            _logger = logger;
            _commissionDashboardEngine = commissionDashboardEngine;
            swapSubscriber.Subscribe(HandleSwapUpdates);
        }

        private async ValueTask HandleSwapUpdates(SwapMessage swapMessage)
        {
            using var activity = MyTelemetry.StartActivity("Handle swap messsge");
            try
            {
                var messageId = swapMessage.Id;
                var dashboardSnapshot = _commissionDashboardEngine.GetTodayDashboardSnapshot();
                var lastBalanceByAsset = dashboardSnapshot
                    .Where(e => e.Asset == swapMessage.DifferenceAsset)
                    .OrderByDescending(e => e.LastUpdateDate)
                    .FirstOrDefault();

                if (lastBalanceByAsset == null ||
                    lastBalanceByAsset.LastMessageId != messageId)
                {
                    await _commissionDashboardEngine.UpdateDashboard(swapMessage, messageId);
                }
                else
                {
                    _logger.LogError($"LiquidityConvertorSwapHandler handle duplicate message. Id = {messageId}");
                }
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                swapMessage.AddToActivityAsJsonTag("mesage");
                throw;
            }
        }

        public void Start()
        {
        }
    }
}