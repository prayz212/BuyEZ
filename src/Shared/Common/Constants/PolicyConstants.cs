namespace Shared.Common.Constants;

public static class PolicyConstants
{
    /*  Customer policies   */
    public const string CUSTOMER_POLICY = "system-user";

    /*  Tenant policies   */
    public const string TENANT_ADMIN_POLICY = "tenant-administrator";
    public const string TENANT_MANAGER_POLICY = "tenant-manager";
    public const string TENANT_STAFF_POLICY = "tenant-staff";

    public const string TENANT_ADMIN_OR_MANAGER_POLICY = "tenant-administrator-manager";
    public const string TENANT_ADMIN_OR_MANAGER_OR_STAFF_POLICY = "tenant-administrator-manager-staff";


    /*  System policies   */
    public const string SYSTEM_ADMIN_POLICY = "system-administrator";
    public const string SYSTEM_SUPPORTER_POLICY = "system-supporter";

    public const string SYSTEM_ADMIN_OR_SUPPORTER_POLICY = "system-administrator-supporter";
}