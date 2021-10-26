using System.Runtime.Serialization;
using Service.Liquidity.DwhDataJob.Domain.Models;

namespace Service.Liquidity.DwhDataJob.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}