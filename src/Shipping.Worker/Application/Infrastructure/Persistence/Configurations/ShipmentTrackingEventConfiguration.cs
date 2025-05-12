using ShippingWorker.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ShippingWorker.Application.Infrastructure.Persistence.Configurations;

public class ShipmentTrackingEventConfiguration : IEntityTypeConfiguration<ShipmentTrackingEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentTrackingEvent> builder)
    {
        builder.HasKey(t => new { t.ShipmentId, t.ExecutionId });

        builder.Property(te => te.CurrentStatus)
            .HasConversion(new EnumToStringConverter<ShipmentStatus>());

        builder.Property(te => te.NewStatus)
            .HasConversion(new EnumToStringConverter<ShipmentStatus>());
    }
}