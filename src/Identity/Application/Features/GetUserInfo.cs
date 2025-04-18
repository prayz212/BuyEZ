using System.Net;
using System.Security.Claims;
using Identity.Application.Shared.RestAPIs;
using IdentityModel;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;

namespace Identity.Application.Features;


public record GetUserInfoResponse(
    [property: JsonProperty(JwtClaimTypes.Subject)] string Id,
    [property: JsonProperty(ClaimTypes.Name)] string Name,
    [property: JsonProperty(ClaimTypes.NameIdentifier)] string Username,
    [property: JsonProperty(ClaimTypes.Email)] string EmailAddress,
    [property: JsonProperty(ClaimTypes.Role)] string Role
);

public record GetUserInfoQuery(string AccessToken) : IRequest<GetUserInfoResponse>;


internal sealed class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuery, GetUserInfoResponse>
{
    private readonly ILogger<GetUserInfoQueryHandler> _logger;
    private readonly IIdentityServerApi _identityServerApi;

    public GetUserInfoQueryHandler(ILogger<GetUserInfoQueryHandler> logger, IIdentityServerApi identityServerApi)
    {
        _logger = logger;
        _identityServerApi = identityServerApi;
    }

    public async Task<GetUserInfoResponse> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request get user info: {@Request}", request);

        try 
        {
            _logger.LogInformation("Calling to Identity Server to get user info");
            var response = await _identityServerApi.GetUserInfoAsync();

            string jsonResponse = JsonConvert.SerializeObject(response);
            var userInfoResponse = JsonConvert.DeserializeObject<GetUserInfoResponse>(jsonResponse);

            return userInfoResponse ?? throw new Exception("Cannot parse user info response object.");
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogError($"Get user info failure: {exception.StatusCode} - {exception.Content}");
            throw new UnauthorizedAccessException("Invalid token.");
        }
        catch (ApiException exception)
        {
            _logger.LogError($"Unhandled get user info failure: {exception.StatusCode} - {exception.Content}");
            throw new Exception("Failed to perform get user info.", exception);
        }
    }
}