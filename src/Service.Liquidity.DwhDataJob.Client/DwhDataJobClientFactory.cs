using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Liquidity.DwhDataJob.Grpc;

namespace Service.Liquidity.DwhDataJob.Client
{
    [UsedImplicitly]
    public class DwhDataJobClientFactory: MyGrpcClientFactory
    {
        public DwhDataJobClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
