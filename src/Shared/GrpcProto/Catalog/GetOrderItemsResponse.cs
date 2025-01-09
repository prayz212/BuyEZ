using ProtoBuf;

namespace Shared.GrpcProto.Catalog;

[ProtoContract]
public record GetOrderItemsResponse
{
    [ProtoMember(1)]
    public required List<ProductReference> Products { get; init; }

    [ProtoMember(2)]
    public required bool IsEnough { get; init; }
}