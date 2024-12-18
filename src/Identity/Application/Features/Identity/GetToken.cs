using FluentValidation;
using Identity.Application.Common;
using Identity.Application.Common.Exceptions;
using Identity.Application.Features.Identity.Shared.RestAPIs;
using MediatR;
using Newtonsoft.Json;

namespace Identity.Application.Features.Identity;

public record GetTokenCommand(string ClientId, string Code, string CodeVerifier) : IRequest<TokenResponse>;

public record TokenResponse(
    [property: JsonProperty("id_token")] string IdToken, 
    [property: JsonProperty("access_token")] string AccessToken, 
    [property: JsonProperty("refresh_token")] string RefreshToken, 
    [property: JsonProperty("expires_in")] int ExpiresIn, 
    [property: JsonProperty("token_type")] string TokenType, 
    [property: JsonProperty("scope")] string Scope);

public class GetTokenCommandValidator : AbstractValidator<GetTokenCommand>
{
    public GetTokenCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.CodeVerifier)
            .NotEmpty().WithMessage("Code Verifier is required.");
    }
}

internal sealed class GetTokenCommandHandler(IIdentityServerApi identityServerApi) : IRequestHandler<GetTokenCommand, TokenResponse>
{
    private readonly IIdentityServerApi _identityServerApi1 = identityServerApi;

    public async Task<TokenResponse> Handle(GetTokenCommand request, CancellationToken cancellationToken)
    {
        // Make a call to connect/token to get the token
        var client = Config.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.ClientId} was not found.");

        var contentKeyValues = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("client_id", request.ClientId),
            new("client_secret", Config.ClientSecretOf(client.ClientId)),
            new("code", request.Code),
            new("redirect_uri", client.RedirectUris.First()),
            new("code_verifier", request.CodeVerifier),
        };

        var content = new FormUrlEncodedContent(contentKeyValues);
        var response = await _identityServerApi1.PostGetTokenAsync(content);

        string jsonResponse = JsonConvert.SerializeObject(response);
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);

        return tokenResponse ?? throw new Exception("Cannot parse token response object.");
    }
}