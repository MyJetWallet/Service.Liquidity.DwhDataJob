using MyJetWallet.Sdk.Postgres;

namespace Service.Liquidity.DwhDataJob.Postgres.DesignTime
{
    public class ContextFactory : MyDesignTimeContextFactory<DatabaseContext>
    {
        public ContextFactory() : base(options => new DatabaseContext(options))
        {

        }
    }
}