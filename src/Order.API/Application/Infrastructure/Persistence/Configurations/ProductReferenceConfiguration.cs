using OrderAPI.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderAPI.Application.Infrastructure.Persistence.Configurations;

public class ProductReferenceConfiguration : IEntityTypeConfiguration<ProductReference>
{
    public void Configure(EntityTypeBuilder<ProductReference> builder)
    {
        builder.ToTable("Product");

        builder.Property(e => e.Price)
            .HasColumnType("decimal(10,2)")
            .HasConversion(
                v => Convert.ToDecimal(v),
                v => Convert.ToDouble(v)
            )
            .IsRequired();
    }
}