using System;

namespace Service.Liquidity.DwhDataJob.Domain.Models
{
    public class CommissionDashboard
    {
        public long Id { get; set; }
        public DateTime CommissionDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Asset { get; set; }
        public decimal Commission { get; set; }
        public string LastMessageId { get; set; }

        public static CommissionDashboard GetCopy(CommissionDashboard entity)
        {
            return new CommissionDashboard()
            {
                CommissionDate = entity.CommissionDate,
                LastUpdateDate = entity.LastUpdateDate,
                Asset = entity.Asset,
                Commission = entity.Commission,
                LastMessageId = entity.LastMessageId
            };
        }
    }
}