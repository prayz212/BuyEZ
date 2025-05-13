using WarehouseWorker.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WarehouseWorker.Application.Infrastructure.Persistence.Configurations;

public class PackageTrackingEventConfiguration : IEntityTypeConfiguration<PackageTrackingEvent>
{
    public void Configure(EntityTypeBuilder<PackageTrackingEvent> builder)
    {
        builder.HasKey(t => new { t.PackageId, t.ExecutionId });

        builder.Property(te => te.CurrentStatus)
            .HasConversion(new EnumToStringConverter<PackageStatus>());

        builder.Property(te => te.NewStatus)
            .HasConversion(new EnumToStringConverter<PackageStatus>());
    }
}