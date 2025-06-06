using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Domain.Interfaces.Repositories;

using Shared.Common;
using Shared.Common.Interfaces;

using Quartz;

namespace WarehouseWorker.BackgroundJobs.Jobs;

public class NotifyShippingVendor : BaseJob<NotifyShippingVendor>
{
    private readonly IPackageRepository _packageRepository;

    public NotifyShippingVendor(
        ILogger<NotifyShippingVendor> logger,
        IJobHistoryRepository jobRepository,
        IPackageRepository packageRepository)
        : base(logger, jobRepository)
    {
        _packageRepository = packageRepository;
    }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var packages = await _packageRepository.GetPackagesByStatus(PackageStatus.Packing);

        _logger.LogInformation("Found {PackageCount} packages need to notify", packages.Count);

        var jobExecutionId = GetJobExecutionId();
        foreach (var package in packages)
        {
            _logger.LogInformation("Notifying shipping vendor for package: {@Package}", package);

            package.MarkOrderReadyForShipment(jobExecutionId);
            _packageRepository.Update(package);
        }

        await _packageRepository.SaveChangesAsync();
    }
}