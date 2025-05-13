using System.ComponentModel.DataAnnotations;

namespace WarehouseWorker.Application.Options;

public class JobCronOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Pack order cron expression is required for schedule job operations.")]
    public required string PackOrder { get; set; }

    [Required(ErrorMessage = "Notify shipping vendor cron expression is required for schedule job operations.")]
    public required string NotifyShippingVendor { get; set; }
}