using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Shared.Common.Constants;

namespace Identity.Application.Common;

public record ClientInfo(string SecretKey, List<string> Roles);

public static class Config
{
    private static Dictionary<string, ClientInfo> ClientInfos 
        => new()
        {
            { 
                "01f9f062-cedb-4a30-877c-c7295ddcc82d", 
                new ClientInfo(
                    SecretKey: "jCp9M7DrMz9mV7efrhhVH7", 
                    Roles: [IdentityConstants.Role.USER])
            },
            { 
                "e3e1e5e3-29cc-4c65-8ed3-4678041d37d7", 
                new ClientInfo(
                    SecretKey: "2wFu6kC3X9Dxh13G2KHADr", 
                    Roles: [IdentityConstants.Role.TENANT_ADMIN, IdentityConstants.Role.TENANT_MANAGER, IdentityConstants.Role.TENANT_STAFF])
            },
            {
                "b285f6f7-c2ac-40e2-a456-cd2d087bf251", 
                new ClientInfo(
                    SecretKey: "6bTAtekG3KZ2ihtPYzAapD", 
                    Roles: [IdentityConstants.Role.SYSTEM_ADMIN, IdentityConstants.Role.SYSTEM_SUPPORT])
            },
        };
    public static string? GetClientSecret(string clientId) 
        => ClientInfos.GetValueOrDefault(clientId)?.SecretKey;

    public static IEnumerable<IdentityResource> IdentityResources => 
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),

            // TODO: Investigate why we need this line
            new (IdentityConstants.StandardScopes.ROLES, new List<string> { "role" }) 
        };

    public static IEnumerable<ApiScope> ApiScopes => 
        new List<ApiScope>
        {
            new (IdentityConstants.StandardScopes.CATALOG_API),
            new (IdentityConstants.StandardScopes.ORDER_API),
            new (IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API),
            new (IdentityConstants.StandardScopes.IDENTITY_API),
        };

    public static IList<ApiResource> ApiResources => 
        new List<ApiResource>
        {
            new (IdentityConstants.StandardScopes.CATALOG_API),
            new (IdentityConstants.StandardScopes.ORDER_API),
            new (IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API),
            new (IdentityConstants.StandardScopes.IDENTITY_API),
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new()
            {
                ClientId = "01f9f062-cedb-4a30-877c-c7295ddcc82d",
                ClientName = "BuyEZ Shopping",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret(ClientInfos["01f9f062-cedb-4a30-877c-c7295ddcc82d"].SecretKey.Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityConstants.StandardScopes.ORDER_API 
                },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,

                // Enables issuing refresh tokens
                AllowOfflineAccess = true,

                // Refresh token settings
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Absolute, 
                AbsoluteRefreshTokenLifetime = 259200, // 3 days
            },
            new()
            {
                ClientId = "e3e1e5e3-29cc-4c65-8ed3-4678041d37d7",
                ClientName = "BuyEZ CRM",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret(ClientInfos["e3e1e5e3-29cc-4c65-8ed3-4678041d37d7"].SecretKey.Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityConstants.StandardScopes.CATALOG_API,
                    IdentityConstants.StandardScopes.ORDER_API, 
                    IdentityConstants.StandardScopes.IDENTITY_API
                },

                // Add custom claims to Id Token
                AlwaysIncludeUserClaimsInIdToken = true,

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
                RefreshTokenExpiration = TokenExpiration.Absolute, 
                AbsoluteRefreshTokenLifetime = 86400, // 1 days
            },
            new()
            {
                ClientId = "b285f6f7-c2ac-40e2-a456-cd2d087bf251",
                ClientName = "BuyEZ Administration",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret(ClientInfos["b285f6f7-c2ac-40e2-a456-cd2d087bf251"].SecretKey.Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityConstants.StandardScopes.CATALOG_API,
                    IdentityConstants.StandardScopes.ORDER_API, 
                    IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API,
                    IdentityConstants.StandardScopes.IDENTITY_API,
                },

                // Add custom claims to Id Token
                AlwaysIncludeUserClaimsInIdToken = true,

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
                RefreshTokenExpiration = TokenExpiration.Absolute, 
                AbsoluteRefreshTokenLifetime = 86400, // 1 days
            }
        };

    public static bool IsInClientRole(string clientId, string role)
    {
        var clientInfo = ClientInfos.GetValueOrDefault(clientId);

        return clientInfo != null 
            ? clientInfo.Roles.Contains(role) 
            : false;
    }
}