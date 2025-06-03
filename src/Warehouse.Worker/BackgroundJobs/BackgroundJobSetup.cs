using WarehouseWorker.Application.Options;
using WarehouseWorker.BackgroundJobs.Jobs;

using Quartz;

namespace WarehouseWorker.BackgroundJobs;

public static class BackgroundJobSetup 
{
    public static QuartzOptions AddQuartzJobs(this QuartzOptions options)
    {
        options
            .AddJob<PackOrder>(jobBuilder => jobBuilder.WithIdentity(nameof(PackOrder)))
            .AddJob<NotifyShippingVendor>(jobBuilder => jobBuilder.WithIdentity(nameof(NotifyShippingVendor)));

        return options;
    }

    public static QuartzOptions AddQuartzTriggers(this QuartzOptions options, JobCronOptions cronOptions)
    {
        options
            .AddTrigger(trigger => trigger.ForJob(nameof(PackOrder)).WithCronSchedule(cronOptions.PackOrder))
            .AddTrigger(trigger => trigger.ForJob(nameof(NotifyShippingVendor)).WithCronSchedule(cronOptions.NotifyShippingVendor));

        return options;
    }
}