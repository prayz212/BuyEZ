using FluentValidation;
using Identity.Application.Common;
using Identity.Application.Common.Exceptions;
using Identity.Application.Features.Identity.Shared.Dtos;
using Identity.Application.Features.Identity.Shared.RestAPIs;
using MediatR;
using Newtonsoft.Json;

namespace Identity.Application.Features.Identity;

public record GetAuthenticationTokenCommand(string ClientId, string Code, string CodeVerifier) : IRequest<AuthenticationTokenResponse>;

public class GetAuthenticationTokenCommandValidator : AbstractValidator<GetAuthenticationTokenCommand>
{
    public GetAuthenticationTokenCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.CodeVerifier)
            .NotEmpty().WithMessage("Code Verifier is required.");
    }
}

internal sealed class GetAuthenticationTokenCommandHandler(IIdentityServerApi identityServerApi) : IRequestHandler<GetAuthenticationTokenCommand, AuthenticationTokenResponse>
{
    private readonly IIdentityServerApi _identityServerApi = identityServerApi;

    public async Task<AuthenticationTokenResponse> Handle(GetAuthenticationTokenCommand request, CancellationToken cancellationToken)
    {
        // Make a call to connect/token to get the token
        var client = Config.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.ClientId} was not found.");

        var contentKeyValues = new Dictionary<string, string>()
        {
            { "client_id", request.ClientId },
            { "client_secret", Config.ClientSecretOf(client.ClientId) },
            { "grant_type", "authorization_code" },
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