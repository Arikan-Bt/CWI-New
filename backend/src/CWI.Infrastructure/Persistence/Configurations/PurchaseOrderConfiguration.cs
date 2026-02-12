using CWI.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWI.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        // Kullanıcı aktiviteleri sorgusunda SupplierId filtresi ve OrderedAt sıralaması kullanılıyor
        builder.HasIndex(po => po.SupplierId);
        builder.HasIndex(po => po.OrderedAt);
    }
}
