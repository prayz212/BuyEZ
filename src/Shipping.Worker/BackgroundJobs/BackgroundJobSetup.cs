using ShippingWorker.Application.Options;
using ShippingWorker.BackgroundJobs.Jobs;

using Quartz;

namespace ShippingWorker.BackgroundJobs;

public static class BackgroundJobSetup 
{
    public static QuartzOptions AddQuartzJobs(this QuartzOptions options)
    {
        options
            .AddJob<FindDriver>(jobBuilder => jobBuilder.WithIdentity(nameof(FindDriver)))
            .AddJob<PickUpOrder>(jobBuilder => jobBuilder.WithIdentity(nameof(PickUpOrder)))
            .AddJob<DeliverOrder>(jobBuilder => jobBuilder.WithIdentity(nameof(DeliverOrder)));

        return options;
    }

    public static QuartzOptions AddQuartzTriggers(this QuartzOptions options, JobCronOptions cronsOptions)
    {
        options
            .AddTrigger(trigger => trigger.ForJob(nameof(FindDriver)).WithCronSchedule(cronsOptions.FindDriver))
            .AddTrigger(trigger => trigger.ForJob(nameof(PickUpOrder)).WithCronSchedule(cronsOptions.PickUpOrder))
            .AddTrigger(trigger => trigger.ForJob(nameof(DeliverOrder)).WithCronSchedule(cronsOptions.DeliverOrder));

        return options;
    }
}