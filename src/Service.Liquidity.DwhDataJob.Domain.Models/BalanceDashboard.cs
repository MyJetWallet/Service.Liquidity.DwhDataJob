using System;
using System.Runtime.CompilerServices;

namespace Service.Liquidity.DwhDataJob.Domain.Models
{
    public class BalanceDashboard
    {
        public long Id { get; set; }
        public DateTime BalanceDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Asset { get; set; }
        public decimal ClientBalance { get; set; }
        public decimal BrokerBalance { get; set; }
        public decimal Commission { get; set; }

        public static BalanceDashboard GetCopy(BalanceDashboard entity)
        {
            return new BalanceDashboard()
            {
                BalanceDate = entity.BalanceDate,
                LastUpdateDate = entity.LastUpdateDate,
                Asset = entity.Asset,
                ClientBalance = entity.ClientBalance,
                BrokerBalance = entity.BrokerBalance,
                Commission = entity.Commission
            };
        }
    }
}