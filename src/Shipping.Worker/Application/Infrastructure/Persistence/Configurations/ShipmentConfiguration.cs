using ShippingWorker.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ShippingWorker.Application.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.TrackingEvents);

        builder.HasMany<ShipmentTrackingEvent>("_trackingEvents")
            .WithOne(te => te.Shipment)
            .HasForeignKey(te => te.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Status)
            .HasConversion(new EnumToStringConverter<ShipmentStatus>());

        builder.Property(e => e.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}