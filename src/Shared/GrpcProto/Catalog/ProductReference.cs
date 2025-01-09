using ProtoBuf;

namespace Shared.GrpcProto.Catalog;

[ProtoContract]
public class ProductReference
{
    [ProtoMember(1)]
    public required string Id { get; init; }

    [ProtoMember(2)]
    public required string Name { get; init; }

    [ProtoMember(3)]
    public required double Price { get; init; }

    [ProtoMember(4)]
    public required int AvailableStock { get; init; }
}