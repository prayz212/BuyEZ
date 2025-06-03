using Shared.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Infrastructure.Persistence.Configurations;

public class JobExecutionHistoryConfiguration : IEntityTypeConfiguration<JobExecutionHistory>
{
    public void Configure(EntityTypeBuilder<JobExecutionHistory> builder)
    {
        builder.Property(j => j.Status)
            .HasConversion(new EnumToStringConverter<ExecutionStatus>());

        builder.Property(e => e.ExecutedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CompletedAt)
            .HasColumnType("timestamp with time zone");
    }
}