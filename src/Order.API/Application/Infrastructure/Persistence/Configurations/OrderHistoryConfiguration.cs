using OrderAPI.Application.Domain.Shopping;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OrderAPI.Application.Infrastructure.Persistence.Configurations;

public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
{
    public void Configure(EntityTypeBuilder<OrderHistory> builder)
    {
        builder.Property(e => e.HistoryStatus)
            .HasConversion(new EnumToStringConverter<OrderStatus>());

        builder.Property(e => e.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}