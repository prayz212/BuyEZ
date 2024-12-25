namespace ClientManagementAPI.Application.Common.Constants;

public static class IdentityConstants
{
    public static class Role
    {
        public const string SYSTEM_ADMIN = "system-admin";
        public const string SYSTEM_SUPPORT = "system-support";

        public const string TENANT_ADMIN = "tenant-admin";
        public const string TENANT_MANAGER = "tenant-manager";
        public const string TENANT_STAFF = "tenant-staff";

        public const string USER = "user";
    }

    public static class StandardScopes
    {
        public const string ROLES = "roles";
        public const string CATALOG_API = "catalog-api";
        public const string CLIENT_MANAGEMENT_API = "client-management-api";
        public const string ORDER_API = "order-api";
        public const string IDENTITY_API = "identity-api";
    }
}
