using System;

namespace Service.Liquidity.DwhDataJob.Domain.Models
{
    public class BalanceDashboard
    {
        public long Id { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Asset { get; set; }
        public decimal ClientBalance { get; set; }
        public decimal BrokerBalance { get; set; }
        public decimal Commission { get; set; }
    }
}