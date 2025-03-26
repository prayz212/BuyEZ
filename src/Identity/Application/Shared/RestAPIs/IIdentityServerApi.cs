using System.Dynamic;
using Refit;

namespace Identity.Application.Shared.RestAPIs;

public interface IIdentityServerApi
{
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    [Post("/connect/token")]
    Task<ExpandoObject> PostGetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] FormUrlEncodedContent encodedContent);

    [Headers("Content-Type: application/x-www-form-urlencoded")]
    [Post("/connect/revocation")]
    Task PostRevokeTokenAsync([Body(BodySerializationMethod.UrlEncoded)] FormUrlEncodedContent encodedContent);
}