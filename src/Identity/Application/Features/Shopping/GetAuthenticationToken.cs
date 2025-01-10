using Identity.Application.Common;
using Identity.Application.Shared.Dtos;
using Identity.Application.Shared.RestAPIs;

using Shared.Common.Exceptions;

using FluentValidation;
using MediatR;
using Newtonsoft.Json;
using Duende.IdentityServer.Models;

namespace Identity.Application.Features.Shopping;


public record GetAuthenticationTokenQuery(string ClientId, string Code, string CodeVerifier) : IRequest<AuthenticationTokenResponse>;


public class GetAuthenticationTokenQueryValidator : AbstractValidator<GetAuthenticationTokenQuery>
{
    public GetAuthenticationTokenQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.CodeVerifier)
            .NotEmpty().WithMessage("Code Verifier is required.");
    }
}


internal sealed class GetAuthenticationTokenQueryHandler(IIdentityServerApi identityServerApi) : IRequestHandler<GetAuthenticationTokenQuery, AuthenticationTokenResponse>
{
    private readonly IIdentityServerApi _identityServerApi = identityServerApi;

    public async Task<AuthenticationTokenResponse> Handle(GetAuthenticationTokenQuery request, CancellationToken cancellationToken)
    {
        // Make a call to connect/token to get the token
        var client = Config.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.ClientId} was not found.");

        var contentKeyValues = new Dictionary<string, string>()
        {
            { "client_id", request.ClientId },
            { "client_secret", Config.ClientSecretOf(client.ClientId) },
            { "grant_type", GrantType.AuthorizationCode },
            { "code", request.Code },
            { "redirect_uri", client.RedirectUris.First() },
            { "code_verifier", request.CodeVerifier },
        };

        var content = new FormUrlEncodedContent(contentKeyValues);
        var response = await _identityServerApi.PostGetTokenAsync(content);

        string jsonResponse = JsonConvert.SerializeObject(response);
        var tokenResponse = JsonConvert.DeserializeObject<AuthenticationTokenResponse>(jsonResponse);

        return tokenResponse ?? throw new Exception("Cannot parse token response object.");
    }
}