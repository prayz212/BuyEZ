using Shared.GrpcProto.Account;

using MediatR;
using ProtoBuf.Grpc;

namespace Identity.Application.Features.Account.gRPC;

public class AccountService(ISender sender) : IAccountService
{
    private readonly ISender _sender = sender;

    public async Task<IdentityAccountDetailResponse> AddIdentityAccountAsync(AddIdentityAccountRequest request, CallContext context = default)
    {   
        return await _sender.Send(new AddIdentityAccountCommand(request));
    }
}