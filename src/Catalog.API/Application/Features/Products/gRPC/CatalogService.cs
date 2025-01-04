using Shared.GrpcProto.Catalog;

using MediatR;
using ProtoBuf.Grpc;

namespace CatalogAPI.Application.Features.Products.gRPC;

public class CatalogService(ISender sender) : ICatalogService
{
    private readonly ISender _sender = sender;

    public async Task<ValidateOrderItemsResponse> ValidateOrderItemsAsync(ValidateOrderItemsRequest request, CallContext context = default)
    {
        return await _sender.Send(new ValidateOrderItemsCommand(request));
    }
}