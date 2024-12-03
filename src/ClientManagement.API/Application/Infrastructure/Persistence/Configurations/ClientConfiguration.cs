using ClientManagementAPI.Application.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.Property(e => e.ValidUntil)
            .HasColumnType("datetime");    

        builder.Property(e => e.Created)
            .HasColumnType("datetime");

        builder.Property(e => e.LastModified)
            .HasColumnType("datetime");

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