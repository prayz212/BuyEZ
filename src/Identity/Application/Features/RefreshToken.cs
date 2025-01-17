using Identity.Application.Common;
using Identity.Application.Shared.Dtos;
using Identity.Application.Shared.RestAPIs;

using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using Refit;
using MediatR;
using System.Net;
using Newtonsoft.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using static Duende.IdentityServer.IdentityServerConstants;

namespace Identity.Application.Features;

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


internal sealed class RefreshTokenQueryHandler(ILogger<RefreshTokenQueryHandler> logger, IIdentityServerApi identityServerApi) : IRequestHandler<RefreshTokenQuery, AuthenticationTokenResponse>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
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
            { "client_secret", Config.GetClientSecret(client.ClientId)! },
            { "grant_type", PersistedGrantTypes.RefreshToken },
            { "refresh_token", request.RefreshToken },
        };

        var content = new FormUrlEncodedContent(contentKeyValues);

        try 
        {
            var response = await _identityServerApi.PostGetTokenAsync(content);

            string jsonResponse = JsonConvert.SerializeObject(response);
            var tokenResponse = JsonConvert.DeserializeObject<AuthenticationTokenResponse>(jsonResponse);

            return tokenResponse ?? throw new Exception("Cannot parse token response object.");
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogError($"Refresh token failure: {exception.StatusCode} - {exception.Content}");
            throw new ValidationException("Invalid grant.");
        }
        catch (ApiException exception)
        {
            _logger.LogError($"Unhandled refresh token failure: {exception.StatusCode} - {exception.Content}");
            throw new Exception("Failed to perform refresh token.", exception);
        }
    }
}
