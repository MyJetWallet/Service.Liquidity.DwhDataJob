using Autofac;
using Service.Liquidity.DwhDataJob.Engines;
using Service.Liquidity.DwhDataJob.Jobs;
using Service.Liquidity.DwhDataJob.Postgres;
using Service.Liquidity.DwhDataJob.SubscriberHandlers;

namespace Service.Liquidity.DwhDataJob.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DatabaseContextFactory>()
                .AsSelf()
                .SingleInstance();
            builder
                .RegisterType<BalanceDashboardEngine>()
                .As<IStartable>()
                .AsSelf()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<IndexPriceJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            builder
                .RegisterType<BalanceDashboardUpdateHandler>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            builder
                .RegisterType<BalanceDashboardPersistJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<MarketPriceEngine>()
                .AsSelf()
                .SingleInstance();
            builder
                .RegisterType<ConvertPriceEngine>()
                .AsSelf()
                .SingleInstance();
        }
    }
}