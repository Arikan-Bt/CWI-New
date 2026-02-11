using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StockAdjustments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [StockAdjustments] (
        [Id] bigint NOT NULL IDENTITY,
        [AdjustmentDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NULL,
        CONSTRAINT [PK_StockAdjustments] PRIMARY KEY ([Id])
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StockAdjustmentItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [StockAdjustmentItems] (
        [Id] bigint NOT NULL IDENTITY,
        [Currency] nvarchar(max) NULL,
        [NewQuantity] int NOT NULL,
        [OldQuantity] int NOT NULL,
        [PackList] nvarchar(max) NULL,
        [Price] decimal(18,2) NULL,
        [ProductId] int NOT NULL,
        [ReceivingNumber] nvarchar(max) NULL,
        [ShelfNumber] nvarchar(max) NULL,
        [StockAdjustmentId] bigint NOT NULL,
        [SupplierName] nvarchar(max) NULL,
        [WarehouseId] int NULL,
        CONSTRAINT [PK_StockAdjustmentItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockAdjustmentItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockAdjustmentItems_StockAdjustments_StockAdjustmentId] FOREIGN KEY ([StockAdjustmentId]) REFERENCES [StockAdjustments] ([Id]) ON DELETE NO ACTION
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[StockMovements]') AND type in (N'U'))
BEGIN
    CREATE TABLE [StockMovements] (
        [Id] bigint NOT NULL IDENTITY,
        [AfterOnHand] int NOT NULL,
        [AfterReserved] int NOT NULL,
        [BeforeOnHand] int NOT NULL,
        [BeforeReserved] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Description] nvarchar(max) NULL,
        [MovementType] int NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [PackList] nvarchar(128) NULL,
        [ProductId] int NOT NULL,
        [QuantityDeltaOnHand] int NOT NULL,
        [QuantityDeltaReserved] int NOT NULL,
        [ReferenceNo] nvarchar(128) NULL,
        [ShelfNumber] nvarchar(128) NULL,
        [SourceDocumentId] bigint NULL,
        [SourceDocumentType] nvarchar(64) NOT NULL,
        [SupplierName] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [WarehouseId] int NULL,
        CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END
");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockAdjustmentItems_ProductId' AND object_id = OBJECT_ID('[StockAdjustmentItems]')) CREATE INDEX [IX_StockAdjustmentItems_ProductId] ON [StockAdjustmentItems] ([ProductId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockAdjustmentItems_StockAdjustmentId' AND object_id = OBJECT_ID('[StockAdjustmentItems]')) CREATE INDEX [IX_StockAdjustmentItems_StockAdjustmentId] ON [StockAdjustmentItems] ([StockAdjustmentId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockMovements_ProductId_OccurredAt' AND object_id = OBJECT_ID('[StockMovements]')) CREATE INDEX [IX_StockMovements_ProductId_OccurredAt] ON [StockMovements] ([ProductId], [OccurredAt]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockMovements_SourceDocument' AND object_id = OBJECT_ID('[StockMovements]')) CREATE INDEX [IX_StockMovements_SourceDocument] ON [StockMovements] ([SourceDocumentType], [SourceDocumentId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StockMovements_Warehouse_Product_OccurredAt' AND object_id = OBJECT_ID('[StockMovements]')) CREATE INDEX [IX_StockMovements_Warehouse_Product_OccurredAt] ON [StockMovements] ([WarehouseId], [ProductId], [OccurredAt]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "StockMovements");
            migrationBuilder.DropTable(name: "StockAdjustmentItems");
            migrationBuilder.DropTable(name: "StockAdjustments");
        }
    }
}
