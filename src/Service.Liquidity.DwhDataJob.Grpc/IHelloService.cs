using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.DwhDataJob.Grpc.Models;

namespace Service.Liquidity.DwhDataJob.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}