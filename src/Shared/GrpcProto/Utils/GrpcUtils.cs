using Shared.Options;
using Grpc.Core;

namespace Shared.GrpcProto.Utils;

public static class GrpcUtils
{
    public static CallOptions GetCallOptions(GrpcBaseOptions options)
    {
        var metaData = new Metadata()
        {
            { "x-api-key", options.ApiKey }
        };

        return new CallOptions(metaData);
    }
}