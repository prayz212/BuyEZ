using ClientManagementAPI.Application.Domain;

using Shared.Common.Enums;
using Shared.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClientManagementAPI.Application.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Ignore(e => e.DomainEvents);
        
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.AliasName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.BriefDescription)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.SubscriptionType)
            .HasConversion(new EnumToStringConverter<SubscriptionType>());

        builder.Property(e => e.RegisteredProductType)
            .HasConversion(new EnumArrayToStringConverter<ProductType>());

        builder.Property(x => x.ValidUntil)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LastModified)
            .HasColumnType("timestamp with time zone");

        builder
            .HasIndex(e => e.AliasName)
            .IsUnique();
    }
}