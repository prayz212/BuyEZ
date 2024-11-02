using CatalogAPI.Application.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Application.Infrastructure.Persistence.Configurations;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {        
        builder.Property(e => e.Filename)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.AltText)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Created)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LastModified)
            .HasColumnType("timestamp with time zone");
    }
}