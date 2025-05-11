using ShippingWorker.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ShippingWorker.Application.Infrastructure.Persistence.Configurations;

public class JobExecutionHistoryConfiguration : IEntityTypeConfiguration<JobExecutionHistory>
{
    public void Configure(EntityTypeBuilder<JobExecutionHistory> builder)
    {
        builder.Ignore(j => j.TrackingEvents);

        builder.HasMany<ShipmentTrackingEvent>("_trackingEvents")
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