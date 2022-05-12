using Autofac;
using MyJetWallet.BitGo.Settings.Ioc;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Bitgo.Watcher.Client;
using Service.ClientWallets.Client;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.InternalWallets.Client;

namespace Service.Liquidity.DwhDataJob.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder
                .CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
            builder.RegisterCurrentPricesClient(myNoSqlClient);
            builder.RegisterConvertIndexPricesClient(myNoSqlClient);
            builder.RegisterBitgoSettingsReader(myNoSqlClient);
            builder.RegisterBitgoWatcherClient(myNoSqlClient);

            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);
            
            
            builder.RegisterMyServiceBusSubscriberSingle<SwapMessage>(serviceBusClient, 
                SwapMessage.TopicName, "DwhDataJob-Swaps", TopicQueueType.PermanentWithSingleConnection);
            
            builder.RegisterClientWalletsClientsWithoutCache(Program.Settings.ClientWalletsGrpcServiceUrl);
            builder.InternalWalletsClient(Program.Settings.InternalWalletsGrpcUrl);
        }
    }
}