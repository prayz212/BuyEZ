using WarehouseWorker.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WarehouseWorker.Application.Infrastructure.Persistence.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.Ignore(s => s.DomainEvents);

        builder.Property(s => s.Status)
            .HasConversion(new EnumToStringConverter<PackageStatus>());

        builder.Property(e => e.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}