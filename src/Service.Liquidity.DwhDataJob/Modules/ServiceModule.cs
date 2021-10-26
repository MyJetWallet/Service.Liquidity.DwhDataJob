using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Service.Liquidity.DwhDataJob.Engines;
using Service.Liquidity.DwhDataJob.Jobs;
using Service.Liquidity.DwhDataJob.Postgres;

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
                .RegisterType<MarketPriceJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<MarketPriceEngine>()
                .AsSelf()
                .SingleInstance();
        }
    }
}