using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.IndexPrices.Client;

namespace Service.Liquidity.DwhDataJob.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterCurrentPricesClient(myNoSqlClient);
        }
    }
}