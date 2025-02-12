namespace Shared.Common;

public static class ApiPaths
{
    public const string Prefix = "api";

    public const string Version = "v{v:apiVersion}";

    public const string Root = $"{Version}/{Prefix}";
}