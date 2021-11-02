using System;

namespace Service.Liquidity.DwhDataJob.Postgres.Models
{
    public class ExternalBalanceEntity
    {
        public long Id { get; set; }
        public DateTime BalanceDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Exchange { get; set; }
        public string Asset { get; set; }
        public decimal Balance { get; set; }
    }
}