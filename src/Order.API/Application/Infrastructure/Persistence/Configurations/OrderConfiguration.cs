using OrderAPI.Application.Domain.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OrderAPI.Application.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(o => o.OrderItems);
        builder.Ignore(o => o.OrderHistories);

        builder.HasMany<OrderItem>("_orderItems")
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<OrderHistory>("_orderHistories")
            .WithOne(oh => oh.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

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