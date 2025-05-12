using ShippingWorker.Application.Options;

using Quartz;
using Microsoft.Extensions.Options;

namespace ShippingWorker.BackgroundJobs;

public class QuartzConfigurationOptions : IConfigureOptions<QuartzOptions>
{
    private readonly JobCronOptions _cronsOptions;

    public QuartzConfigurationOptions(IOptions<JobCronOptions> cronsOptions)
    {
        _cronsOptions = cronsOptions.Value;
    }

    public void Configure(QuartzOptions options)
    {
        options
            .AddQuartzJobs()
            .AddQuartzTriggers(_cronsOptions);
    }
}