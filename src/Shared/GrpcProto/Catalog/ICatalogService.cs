using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Shared.GrpcProto.Catalog;

[Service]
public interface ICatalogService
{
    [Operation]
    Task<ValidateOrderItemsResponse> ValidateOrderItemsAsync(ValidateOrderItemsRequest request, CallContext context = default);
}