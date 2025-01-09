using OrderAPI.Application.Domain.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderAPI.Application.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(e => e.ProductPrice)
            .HasColumnType("decimal(9,2)")
            .HasConversion(
                v => Convert.ToDecimal(v),
                v => Convert.ToDouble(v)
            )
            .IsRequired();

        builder.Property(e => e.TotalPrice)
            .HasColumnType("decimal(10,2)")
            .HasConversion(
                v => Convert.ToDecimal(v),
                v => Convert.ToDouble(v)
            )
            .IsRequired();

        builder.Property(x => x.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}