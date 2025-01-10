using ProtoBuf;

namespace Shared.GrpcProto.Account;

[ProtoContract]
public record AddIdentityAccountPayload
{
    [ProtoMember(1)]
    public required string TenantId { get; init; }

    [ProtoMember(2)]
    public required string FirstName { get; init; }

    [ProtoMember(3)]
    public required string LastName { get; init; }

    [ProtoMember(4)]
    public required string UserName { get; init; }

    [ProtoMember(5)]
    public required string Email { get; init; }

    [ProtoMember(6)]
    public required string Role { get; init; }

    [ProtoMember(7)]
    public required string RequestingUserId { get; init; }
}