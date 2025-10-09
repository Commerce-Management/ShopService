using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopService.Core.Entities;

namespace ShopService.Infrastructure.Configs;

public class ShopConfig : IEntityTypeConfiguration<Shop>
{
       public void Configure(EntityTypeBuilder<Shop> builder)
       {
              builder.HasKey(x => x.Id);

              builder.Property(x => x.Id)
                     .HasColumnType("uuid");

              builder.Property(x => x.Name)
                     .IsRequired()
                     .HasMaxLength(100)
                     .HasColumnType("text");

              builder.Property(x => x.Description)
                     .IsRequired()
                     .HasMaxLength(500)
                     .HasColumnType("text");

              builder.Property(x => x.ContactEmail)
                     .IsRequired()
                     .HasMaxLength(100)
                     .HasColumnType("text");

              builder.Property(x => x.ContactPhone)
                     .IsRequired()
                     .HasMaxLength(20)
                     .HasColumnType("text");

              builder.Property(x => x.Subdomain)
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasColumnType("text");

              builder.Property(x => x.CreatedAt)
                     .IsRequired()
                     .HasColumnType("timestamp with time zone");

              builder.Property(x => x.UpdatedAt)
                     .HasColumnType("timestamp with time zone")
                     .IsRequired(false);

              builder.Property(x => x.IsActive)
                     .IsRequired()
                     .HasColumnType("boolean");

              builder.Property(x => x.OwnerUserId)
                     .HasColumnType("uuid")
                     .IsRequired();

              builder.HasIndex(x => x.Subdomain)
                     .IsUnique();

              builder.HasIndex(x => x.OwnerUserId);
       }
}