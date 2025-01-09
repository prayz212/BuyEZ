using Shared.GrpcProto.Catalog;

using MediatR;
using ProtoBuf.Grpc;

namespace CatalogAPI.Application.Features.gRPC;

public class CatalogService(ISender sender) : ICatalogService
{
    private readonly ISender _sender = sender;

    public async Task<GetOrderItemsResponse> GetOrderItemsAsync(GetOrderItemsPayload payload, CallContext context = default)
    {
        return await _sender.Send(new GetOrderItemsQuery(payload));
    }
}