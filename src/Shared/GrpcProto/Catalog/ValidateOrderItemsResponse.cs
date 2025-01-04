using ProtoBuf;

namespace Shared.GrpcProto.Catalog;

[ProtoContract]
public record ValidateOrderItemsResponse
{
    [ProtoMember(1)]
    public required List<ProductReference> Products { get; init; }

    [ProtoMember(2)]
    public required bool IsEnough { get; init; }
}