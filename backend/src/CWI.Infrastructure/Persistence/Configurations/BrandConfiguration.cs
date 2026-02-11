using CWI.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWI.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasIndex(b => b.Name);
        builder.Property(b => b.Name).HasMaxLength(250).IsRequired();
        builder.Property(b => b.Code).HasMaxLength(50).IsRequired();
    }
}
