using Service.IndexPrices.Domain.Models;

namespace Service.Liquidity.DwhDataJob.Postgres.Models
{
    public class MarketPriceEntity : CurrentPrice
    {
        public long Id { get; set; }

        public MarketPriceEntity()
        {
        }

        public MarketPriceEntity(CurrentPrice baseEntity)
        {
            this.BrokerId = baseEntity.BrokerId;
            this.InstrumentSymbol = baseEntity.InstrumentSymbol;
            this.DateTime = baseEntity.DateTime;
            this.Price = baseEntity.Price;
            this.InstrumentStatus = baseEntity.InstrumentStatus;
            this.Source = baseEntity.Source;
            this.SourceMarket = baseEntity.SourceMarket;
        }
    }
}