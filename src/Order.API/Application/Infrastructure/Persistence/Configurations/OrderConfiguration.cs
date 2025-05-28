using OrderAPI.Application.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OrderAPI.Application.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Ignore(e => e.DomainEvents);

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(10,2)")
            .HasConversion(
                v => Convert.ToDecimal(v),
                v => Convert.ToDouble(v)
            )
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(new EnumToStringConverter<OrderStatus>());

        builder.Property(e => e.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}