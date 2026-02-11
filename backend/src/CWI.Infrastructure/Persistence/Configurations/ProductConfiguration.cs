using CWI.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWI.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(500).IsRequired();
        
        builder.HasIndex(p => p.IsActive);
        
        // Marka ile joinler için indeks
        builder.HasIndex(p => p.BrandId);
    }
}
