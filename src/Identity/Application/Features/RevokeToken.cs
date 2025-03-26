using Identity.Application.Common;
using Identity.Application.Shared.RestAPIs;

using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using Refit;
using MediatR;
using System.Net;
using FluentValidation;
using Microsoft.Extensions.Logging;
using static Duende.IdentityServer.IdentityServerConstants;

namespace Identity.Application.Features;


public record RevokeTokenQuery(string ClientId, string RefreshToken) : IRequest;


public class RevokeTokenQueryValidator : AbstractValidator<RevokeTokenQuery>
{
    public RevokeTokenQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is required.");
    }
}


internal sealed class RevokeTokenQueryHandler : IRequestHandler<RevokeTokenQuery>
{
    private readonly ILogger<RevokeTokenQueryHandler> _logger;
    private readonly IIdentityServerApi _identityServerApi;


    public RevokeTokenQueryHandler(ILogger<RevokeTokenQueryHandler> logger, IIdentityServerApi identityServerApi)
    {
        _logger = logger;
        _identityServerApi = identityServerApi;
    }

    public async Task Handle(RevokeTokenQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request token revocation.");

        var client = Config.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.ClientId} was not found.");

        var clientSecret = Config.GetClientSecret(request.ClientId)!;
        var contentKeyValues = new Dictionary<string, string>()
        {
            { "client_id", request.ClientId },
            { "client_secret", clientSecret },
            { "token", request.RefreshToken },
            { "token_type_hint", PersistedGrantTypes.RefreshToken },
        };

        var content = new FormUrlEncodedContent(contentKeyValues);

        try
        {
            _logger.LogInformation("Calling to Identity Server to revoke token: {@Payload}", contentKeyValues.Where(kv => kv.Key != "token"));
            await _identityServerApi.PostRevokeTokenAsync(content);
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogError($"Revoke token failure: {exception.StatusCode} - {exception.Content}");
            throw new ValidationException("Invalid token.");
        }
        catch (ApiException exception)
        {
            _logger.LogError($"Unhandled revoke token failure: {exception.StatusCode} - {exception.Content}");
            throw new Exception("Failed to perform token revocation.", exception);
        }
    }
}