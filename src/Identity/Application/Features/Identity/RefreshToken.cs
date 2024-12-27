using Identity.Application.Common;
using Identity.Application.Features.Identity.Shared.Dtos;
using Identity.Application.Features.Identity.Shared.RestAPIs;

using Shared.Common.Exceptions;

using FluentValidation;
using MediatR;
using Newtonsoft.Json;

namespace Identity.Application.Features.Identity;

public record RefreshTokenCommand(string ClientId, string RefreshToken) : IRequest<AuthenticationTokenResponse>;


public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is required.");
    }
}


internal sealed class RefreshTokenCommandHandler(IIdentityServerApi identityServerApi) : IRequestHandler<RefreshTokenCommand, AuthenticationTokenResponse>
{
    private readonly IIdentityServerApi _identityServerApi = identityServerApi;

    public async Task<AuthenticationTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
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
