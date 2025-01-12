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

namespace Identity.Application.Features.Shopping;


public record ExchangeAuthorizationCodeQuery(string ClientId, string Code, string CodeVerifier) : IRequest<AuthenticationTokenResponse>;


public class ExchangeAuthorizationCodeQueryValidator : AbstractValidator<ExchangeAuthorizationCodeQuery>
{
    public ExchangeAuthorizationCodeQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client Id is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.CodeVerifier)
            .NotEmpty().WithMessage("Code Verifier is required.");
    }
}


internal sealed class ExchangeAuthorizationCodeQueryHandler(ILogger<ExchangeAuthorizationCodeQueryHandler> logger, IIdentityServerApi identityServerApi) : IRequestHandler<ExchangeAuthorizationCodeQuery, AuthenticationTokenResponse>
{
    private readonly ILogger<ExchangeAuthorizationCodeQueryHandler> _logger = logger;
    private readonly IIdentityServerApi _identityServerApi = identityServerApi;

    public async Task<AuthenticationTokenResponse> Handle(ExchangeAuthorizationCodeQuery request, CancellationToken cancellationToken)
    {
        // Make a call to connect/token to get the token
        var client = Config.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.ClientId} was not found.");

        var contentKeyValues = new Dictionary<string, string>()
        {
            { "client_id", request.ClientId },
            { "client_secret", Config.GetClientSecret(client.ClientId)! },
            { "grant_type", PersistedGrantTypes.AuthorizationCode },
            { "code", request.Code },
            { "redirect_uri", client.RedirectUris.First() },
            { "code_verifier", request.CodeVerifier },
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
            _logger.LogError($"Exchange authorization code failure: {exception.StatusCode} - {exception.Content}");
            throw new ValidationException("Invalid grant.");
        }
        catch (ApiException exception)
        {
            _logger.LogError($"Unhandled exchange authorization code failure: {exception.StatusCode} - {exception.Content}");
            throw new Exception("Failed to perform authorization code exchange.", exception);
        }
    }
}