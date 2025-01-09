using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Shared.GrpcProto.Catalog;

[Service]
public interface ICatalogService
{
    [Operation]
    Task<GetOrderItemsResponse> GetOrderItemsAsync(GetOrderItemsPayload payload, CallContext context = default);
}