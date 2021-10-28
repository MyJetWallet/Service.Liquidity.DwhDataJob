using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;

namespace Service.Liquidity.DwhDataJob.Client
{
    [UsedImplicitly]
    public class DwhDataJobClientFactory: MyGrpcClientFactory
    {
        public DwhDataJobClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }
    }
}
