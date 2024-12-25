using Identity.Application.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientManagementAPI.Application.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {     
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