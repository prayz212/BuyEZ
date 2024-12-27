using Shared.Options;
using Grpc.Core;

namespace ClientManagementAPI.Application.Utils;

public static class GrpcUtils
{
    public static CallOptions GetCallOptions<TService>(GrpcOptions options)
    {
        var metaData = new Metadata()
        {
            { "x-api-key", options.ApiKey }
        };

        return new CallOptions(metaData);
    }
}