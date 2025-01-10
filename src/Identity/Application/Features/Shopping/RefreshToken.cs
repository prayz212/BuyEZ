using Identity.Application.Common;
using Identity.Application.Shared.Dtos;
using Identity.Application.Shared.RestAPIs;

using Shared.Common.Exceptions;

using FluentValidation;
using MediatR;
using Newtonsoft.Json;

namespace Identity.Application.Features.Shopping;


public record RefreshTokenQuery(string ClientId, string RefreshToken) : IRequest<AuthenticationTokenResponse>;


public class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is required.");
    }
}


internal sealed class RefreshTokenQueryHandler(IIdentityServerApi identityServerApi) : IRequestHandler<RefreshTokenQuery, AuthenticationTokenResponse>
{
    private readonly IIdentityServerApi _identityServerApi = identityServerApi;

    public async Task<AuthenticationTokenResponse> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        // Make a call to connect/token to get the token
        var client = Config.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.ClientId} was not found.");   

        var contentKeyValues = new Dictionary<string, string>()
        {
            { "client_id", request.ClientId },
            { "client_secret", Config.ClientSecretOf(client.ClientId) },
            { "grant_type", "refresh_token" },
            { "refresh_token", request.RefreshToken },
        };

        var content = new FormUrlEncodedContent(contentKeyValues);
        var response = await _identityServerApi.PostGetTokenAsync(content);

        string jsonResponse = JsonConvert.SerializeObject(response);
        var tokenResponse = JsonConvert.DeserializeObject<AuthenticationTokenResponse>(jsonResponse);

        return tokenResponse ?? throw new Exception("Cannot parse token response object.");
    }
}
