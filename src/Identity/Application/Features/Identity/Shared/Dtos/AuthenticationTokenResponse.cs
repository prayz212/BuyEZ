using Newtonsoft.Json;

namespace Identity.Application.Features.Identity.Shared.Dtos;

public record AuthenticationTokenResponse(
    [property: JsonProperty("id_token")] string IdToken, 
    [property: JsonProperty("access_token")] string AccessToken, 
    [property: JsonProperty("refresh_token")] string RefreshToken, 
    [property: JsonProperty("expires_in")] int ExpiresIn, 
    [property: JsonProperty("token_type")] string TokenType, 
    [property: JsonProperty("scope")] string Scope);