using CatalogAPI.Application.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Application.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Ignore(e => e.DomainEvents);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Price)
            .HasColumnType("decimal(9,2)");

        builder.Property(e => e.Created)
            .HasColumnType("datetime");

        builder.Property(e => e.LastModified)
            .HasColumnType("datetime");

        builder.Property(x => x.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}