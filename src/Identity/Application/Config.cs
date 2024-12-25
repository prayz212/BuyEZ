using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.Application.Common;

public static class Config
{
    private static List<KeyValuePair<string, string>> ClientKeyValues 
        => new List<KeyValuePair<string, string>>
        {
            new("01f9f062-cedb-4a30-877c-c7295ddcc82d", "jCp9M7DrMz9mV7efrhhVH7"),
            new("e3e1e5e3-29cc-4c65-8ed3-4678041d37d7", "2wFu6kC3X9Dxh13G2KHADr"),
            new("b285f6f7-c2ac-40e2-a456-cd2d087bf251", "6bTAtekG3KZ2ihtPYzAapD")
        };
    public static string ClientSecretOf(string clientId) 
        => ClientKeyValues.FirstOrDefault(kv => kv.Key == clientId).Value;

    public static IEnumerable<IdentityResource> IdentityResources => 
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),

            // TODO: Investigate why we need this line
            new (Constants.IdentityConstants.StandardScopes.ROLES, new List<string> { "role" }) 
        };

    public static IEnumerable<ApiScope> ApiScopes => 
        new List<ApiScope>
        {
            new (Constants.IdentityConstants.StandardScopes.CATALOG_API),
            new (Constants.IdentityConstants.StandardScopes.ORDER_API),
            new (Constants.IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API),
            new (Constants.IdentityConstants.StandardScopes.IDENTITY_API),
        };

    public static IList<ApiResource> ApiResources => 
        new List<ApiResource>
        {
            new (Constants.IdentityConstants.StandardScopes.CATALOG_API),
            new (Constants.IdentityConstants.StandardScopes.ORDER_API),
            new (Constants.IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API),
            new (Constants.IdentityConstants.StandardScopes.IDENTITY_API),
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new()
            {
                ClientId = ClientKeyValues[0].Key,
                ClientName = "BuyEZ Shopping",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret(ClientKeyValues[0].Value.Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Constants.IdentityConstants.StandardScopes.ORDER_API 
                },

                // Where to redirect after login
                RedirectUris = { "http://localhost:9000/auth/signin-oidc" },

                // Where to redirect after logout
                PostLogoutRedirectUris = { "http://localhost:9000/auth/signout-callback-oidc" },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,

                // Enables issuing refresh tokens
                AllowOfflineAccess = true,

                // Refresh token settings
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding, 
                AbsoluteRefreshTokenLifetime = 172800, // 2 days
                SlidingRefreshTokenLifetime = 21600  // 6 hours
            },
            new()
            {
                ClientId = ClientKeyValues[1].Key,
                ClientName = "BuyEZ CRM",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret(ClientKeyValues[1].Value.Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Constants.IdentityConstants.StandardScopes.CATALOG_API,
                    Constants.IdentityConstants.StandardScopes.ORDER_API, 
                    Constants.IdentityConstants.StandardScopes.IDENTITY_API
                },

                // Where to redirect after login
                RedirectUris = { "http://localhost:9100/auth/signin-oidc" },

                // Where to redirect after logout
                PostLogoutRedirectUris = { "http://localhost:9100/auth/signout-callback-oidc" },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,

                // Enables issuing refresh tokens
                AllowOfflineAccess = true,

                // Refresh token settings
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding, 
                AbsoluteRefreshTokenLifetime = 172800, // 2 days
                SlidingRefreshTokenLifetime = 21600  // 6 hours
            },
            new()
            {
                ClientId = ClientKeyValues[2].Key,
                ClientName = "BuyEZ Administration",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret(ClientKeyValues[2].Value.Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    Constants.IdentityConstants.StandardScopes.CATALOG_API,
                    Constants.IdentityConstants.StandardScopes.ORDER_API, 
                    Constants.IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API,
                    Constants.IdentityConstants.StandardScopes.IDENTITY_API,
                },

                // Where to redirect after login
                RedirectUris = { "http://localhost:9100/auth/signin-oidc" },

                // Where to redirect after logout
                PostLogoutRedirectUris = { "http://localhost:9100/auth/signout-callback-oidc" },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,

                // Enables issuing refresh tokens
                AllowOfflineAccess = true,

                // Refresh token settings
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding, 
                AbsoluteRefreshTokenLifetime = 172800, // 2 days
                SlidingRefreshTokenLifetime = 21600  // 6 hours
            }
        };
}