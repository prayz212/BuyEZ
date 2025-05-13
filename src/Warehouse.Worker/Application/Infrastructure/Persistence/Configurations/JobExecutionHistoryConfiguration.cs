using WarehouseWorker.Application.Domain;

using Shared.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WarehouseWorker.Application.Infrastructure.Persistence.Configurations;

public class JobExecutionHistoryConfiguration : IEntityTypeConfiguration<JobExecutionHistory<PackageTrackingEvent>>
{
    public void Configure(EntityTypeBuilder<JobExecutionHistory<PackageTrackingEvent>> builder)
    {
        builder.Ignore(j => j.TrackingEvents);

        builder.HasMany<PackageTrackingEvent>("_trackingEvents")
            .WithOne(te => te.ExecutionHistory)
            .HasForeignKey(te => te.ExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(j => j.Status)
            .HasConversion(new EnumToStringConverter<ExecutionStatus>());

        builder.Property(e => e.ExecutedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CompletedAt)
            .HasColumnType("timestamp with time zone");
    }
}