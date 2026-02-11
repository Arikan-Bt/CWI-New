using CWI.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWI.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Temel property ayarları
        builder.Property(oi => oi.ProductName).HasMaxLength(500).IsRequired();
        
        // İlişki (Foreign Key) İndeksleri
        // Sipariş detaylarını çekerken OrderId üzerinden çok sık sorgu atılır
        builder.HasIndex(oi => oi.OrderId);
        
        // Ürün bazlı raporlamalar için
        builder.HasIndex(oi => oi.ProductId);
        
        // Rapor sorguları için composite index - GroupBy performansını artırır
        builder.HasIndex(oi => new { oi.OrderId, oi.ProductId })
            .HasDatabaseName("IX_OrderItems_OrderId_ProductId_Composite");
    }
}
