using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Domain.Interfaces.Repositories;

using Shared.Common;

using Quartz;
using Shared.Common.Interfaces;

namespace WarehouseWorker.BackgroundJobs.Jobs;

public class PackOrder : BaseJob<PackOrder>
{
    private readonly IPackageRepository _packageRepository;

    public PackOrder(
        ILogger<PackOrder> logger,
        IJobHistoryRepository jobRepository,
        IPackageRepository packageRepository)
        : base(logger, jobRepository)
    {
        _packageRepository = packageRepository;
    }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var packages = await _packageRepository.GetPackagesByStatus(PackageStatus.Pending);

        _logger.LogInformation("Found {PackageCount} orders need to pack", packages.Count);

        var jobExecutionId = GetJobExecutionId();
        foreach (var package in packages)
        {
            _logger.LogInformation("Packing order: {@Package}", package);

            package.PackOrder(jobExecutionId);
            _packageRepository.Update(package);
        }

        await _packageRepository.SaveChangesAsync();
    }
}