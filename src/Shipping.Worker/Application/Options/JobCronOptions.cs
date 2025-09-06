using System.ComponentModel.DataAnnotations;

namespace ShippingWorker.Application.Options;

public class JobCronOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Find driver cron expression is required for schedule job operations.")]
    public required string FindDriver { get; set; }

    [Required(ErrorMessage = "Pick up order cron expression is required for schedule job operations.")]
    public required string PickUpOrder { get; set; }

    [Required(ErrorMessage = "Deliver order cron expression is required for schedule job operations.")]
    public required string DeliverOrder { get; set; }
    
    [Required(ErrorMessage = "Deliver outcome cron expression is required for schedule job operations.")]
    public required string DeliverOutcome { get; set; }
}