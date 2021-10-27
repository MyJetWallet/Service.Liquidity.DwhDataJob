using Service.IndexPrices.Domain.Models;

namespace Service.Liquidity.DwhDataJob.Postgres.Models
{
    public class ConvertIndexPriceEntity : ConvertIndexPrice
    {
        public long Id { get; set; }

        public ConvertIndexPriceEntity()
        {
        }

        public ConvertIndexPriceEntity(ConvertIndexPrice baseEntity)
        {
            this.BaseAsset = baseEntity.BaseAsset;
            this.QuotedAsset = baseEntity.QuotedAsset;
            this.Price = baseEntity.Price;
            this.UpdateDate = baseEntity.UpdateDate;
            this.Error = baseEntity.Error;
        }
    }
}