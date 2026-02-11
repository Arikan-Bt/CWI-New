
-- 1. Create a new Warehouse
INSERT INTO dbo.Warehouses (Code, Name, IsActive, IsDefault, CreatedAt, UpdatedAt)
VALUES ('2', 'Yedek Depo', 1, 0, GETDATE(), GETDATE());

DECLARE @NewWarehouseId INT = SCOPE_IDENTITY();

-- 2. Get some products from an existing order (e.g., Order 887)
-- We use products that are in the order 887 based on previous checks.

-- Insert/Update Inventory for Warehouse 1 (Main)
MERGE dbo.InventoryItems AS target
USING (SELECT TOP 3 ProductId FROM dbo.OrderItems WHERE OrderId = 887) AS source
ON (target.WarehouseId = 1 AND target.ProductId = source.ProductId)
WHEN MATCHED THEN
    UPDATE SET target.QuantityOnHand = target.QuantityOnHand + 100
WHEN NOT MATCHED THEN
    INSERT (WarehouseId, ProductId, QuantityOnHand, QuantityReserved, UpdatedAt)
    VALUES (1, source.ProductId, 100, 0, GETDATE());

-- Insert Inventory for Warehouse 2 (New) for the SAME products
INSERT INTO dbo.InventoryItems (WarehouseId, ProductId, QuantityOnHand, QuantityReserved, UpdatedAt)
SELECT @NewWarehouseId, ProductId, 50, 0, GETDATE()
FROM dbo.OrderItems
WHERE OrderId = 887
GROUP BY ProductId
ORDER BY ProductId
OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY;

-- 3. Verify the data
SELECT w.Name as Warehouse, p.Sku, ii.QuantityOnHand, ii.QuantityReserved
FROM dbo.InventoryItems ii
JOIN dbo.Warehouses w ON ii.WarehouseId = w.Id
JOIN dbo.Products p ON ii.ProductId = p.Id
WHERE ii.ProductId IN (SELECT TOP 3 ProductId FROM dbo.OrderItems WHERE OrderId = 887)
ORDER BY p.Sku, w.Name;
