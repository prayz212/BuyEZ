using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.Application.Common;

public static class Config
{
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
                ClientId = "01f9f062-cedb-4a30-877c-c7295ddcc82d",
                ClientName = "BuyEZ Shopping",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret("jCp9M7DrMz9mV7efrhhVH7".Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    Constants.IdentityConstants.StandardScopes.ORDER_API, 
                },
                // Where to redirect after login
                RedirectUris = { "http://localhost:9000/auth/signin-oidc" },

                // Where to redirect after logout
                PostLogoutRedirectUris = { "http://localhost:9000/auth/signout-callback-oidc" },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,
            },
            new()
            {
                ClientId = "e3e1e5e3-29cc-4c65-8ed3-4678041d37d7",
                ClientName = "BuyEZ CRM",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret("2wFu6kC3X9Dxh13G2KHADr".Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    Constants.IdentityConstants.StandardScopes.CATALOG_API,
                    Constants.IdentityConstants.StandardScopes.ORDER_API, 
                    Constants.IdentityConstants.StandardScopes.IDENTITY_API
                },
                // Where to redirect after login
                // RedirectUris = { "https://localhost:44330/signin-oidc" },

                // Where to redirect after logout
                // PostLogoutRedirectUris = { "https://localhost:44330/signout-callback-oidc" },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,
            },
            new()
            {
                ClientId = "b285f6f7-c2ac-40e2-a456-cd2d087bf251",
                ClientName = "BuyEZ Administration",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets = { new Secret("6bTAtekG3KZ2ihtPYzAapD".Sha256()) },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    Constants.IdentityConstants.StandardScopes.CATALOG_API,
                    Constants.IdentityConstants.StandardScopes.ORDER_API, 
                    Constants.IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API,
                    Constants.IdentityConstants.StandardScopes.IDENTITY_API
                },
                // Where to redirect after login
                // RedirectUris = { "https://localhost:44330/signin-oidc" },

                // Where to redirect after logout
                // PostLogoutRedirectUris = { "https://localhost:44330/signout-callback-oidc" },

                // Authorization
                AccessTokenLifetime = 3600, 

                // Authentication
                IdentityTokenLifetime = 3600,
            }
        };
}