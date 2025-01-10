using Shared.GrpcProto.Account;

using MediatR;
using ProtoBuf.Grpc;

namespace Identity.Application.Features.Administration.gRPC;

public class AccountService(ISender sender) : IAccountService
{
    private readonly ISender _sender = sender;

    public async Task<IdentityAccountDetailResponse> AddIdentityAccountAsync(AddIdentityAccountPayload request, CallContext context = default)
    {   
        return await _sender.Send(new AddIdentityAccountCommand(request));
    }
}