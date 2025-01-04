using ProtoBuf;

namespace Shared.GrpcProto.Catalog;

[ProtoContract]
public record ValidateOrderItemsRequest
{
    [ProtoMember(1)]
    public required List<string> Ids { get; init; }
}