using Identity.Application.Common;
using Identity.Application.Shared.RestAPIs;

using ValidationException = Shared.Common.Exceptions.ValidationException;

using Refit;
using MediatR;
using System.Net;
using Newtonsoft.Json;
using FluentValidation;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Shopping;


public record AuthenticateUserResponse(
    [property: JsonProperty("access_token")] string AccessToken, 
    [property: JsonProperty("refresh_token")] string RefreshToken, 
    [property: JsonProperty("expires_in")] int ExpiresIn, 
    [property: JsonProperty("token_type")] string TokenType, 
    [property: JsonProperty("scope")] string Scope);

public record AuthenticateUserQuery(string Username, string Password) : IRequest<AuthenticateUserResponse>;


public class AuthenticateUserQueryValidator : AbstractValidator<AuthenticateUserQuery>
{
    public AuthenticateUserQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Must(NotContainSpaceBetween).WithMessage("Username must not contain spaces between.");

        // TODO: implement password strong validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must have at least 6 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");
    }

    private bool NotContainSpaceBetween(string username) => 
        !username.Contains(' ');
}


internal sealed class AuthenticateUserQueryHandler : IRequestHandler<AuthenticateUserQuery, AuthenticateUserResponse>
{
    private readonly ILogger<AuthenticateUserQueryHandler> _logger;
    private readonly IIdentityServerApi _identityServerApi;
    private readonly Client _client;

    public AuthenticateUserQueryHandler(ILogger<AuthenticateUserQueryHandler> logger, IIdentityServerApi identityServerApi)
    {
        _logger = logger;
        _identityServerApi = identityServerApi;

        /* Hard coding the BuyEZ Shopping ClientId */
        var clientId = "01f9f062-cedb-4a30-877c-c7295ddcc82d";
        _client = Config.Clients.First(c => c.ClientId == clientId);
    }

    public async Task<AuthenticateUserResponse> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        // Make a call to connect/token to get the token
        var contentKeyValues = new Dictionary<string, string>()
        {
            { "client_id", _client.ClientId },
            { "client_secret", Config.GetClientSecret(_client.ClientId)! },
            { "grant_type", GrantType.ResourceOwnerPassword },
            { "username", request.Username },
            { "password", request.Password },
            { "scope", string.Join(" ", _client.AllowedScopes) }
        };

        var content = new FormUrlEncodedContent(contentKeyValues);

        try 
        {
            var response = await _identityServerApi.PostGetTokenAsync(content);

            string jsonResponse = JsonConvert.SerializeObject(response);
            var tokenResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(jsonResponse);

            return tokenResponse ?? throw new Exception("Cannot parse token response object.");
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogError($"Authenticate user failure: {exception.StatusCode} - {exception.Content}");
            throw new ValidationException("Invalid credentials.");
        }
        catch (ApiException exception)
        {
            _logger.LogError($"Unhandled authenticate user failure: {exception.StatusCode} - {exception.Content}");
            throw new Exception("Failed to perform user authentication in ROP flow.", exception);
        }
    }
}

