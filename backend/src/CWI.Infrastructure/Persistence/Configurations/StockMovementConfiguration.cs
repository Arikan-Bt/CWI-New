using CWI.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWI.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.Property(x => x.SourceDocumentType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(128);
        builder.Property(x => x.ShelfNumber).HasMaxLength(128);
        builder.Property(x => x.PackList).HasMaxLength(128);
        builder.Property(x => x.SupplierName).HasMaxLength(256);

        builder.HasIndex(x => new { x.ProductId, x.OccurredAt })
            .HasDatabaseName("IX_StockMovements_ProductId_OccurredAt");

        builder.HasIndex(x => new { x.WarehouseId, x.ProductId, x.OccurredAt })
            .HasDatabaseName("IX_StockMovements_Warehouse_Product_OccurredAt");

        builder.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId })
            .HasDatabaseName("IX_StockMovements_SourceDocument");
    }
}
