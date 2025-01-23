using Identity.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Application.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {     
        builder.Property(x => x.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}