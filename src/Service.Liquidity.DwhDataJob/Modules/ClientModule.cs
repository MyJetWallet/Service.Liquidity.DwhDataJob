using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.ClientWallets.Client;
using Service.IndexPrices.Client;
using Service.MatchingEngine.EventBridge.ServiceBus;

namespace Service.Liquidity.DwhDataJob.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder
                .CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterCurrentPricesClient(myNoSqlClient);
            builder.RegisterConvertIndexPricesClient(myNoSqlClient);

            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);
            builder.RegisterMeEventSubscriber(serviceBusClient, "dwh-data-job", TopicQueueType.PermanentWithSingleConnection);
            
            builder.RegisterClientWalletsClientsWithoutCache(Program.Settings.ClientWalletsGrpcServiceUrl);
        }
    }
}