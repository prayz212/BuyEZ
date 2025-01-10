using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Shared.GrpcProto.Account;

[Service]
public interface IAccountService
{
    [Operation]
    Task<IdentityAccountDetailResponse> AddIdentityAccountAsync(AddIdentityAccountPayload request, CallContext context = default);
}