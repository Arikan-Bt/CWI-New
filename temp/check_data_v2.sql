
SELECT TOP 5 Id, Name, Code FROM dbo.Warehouses;
SELECT TOP 5 oi.OrderId, oi.ProductId, p.Sku, p.Name, oi.Quantity 
FROM dbo.OrderItems oi
JOIN dbo.Products p ON oi.ProductId = p.Id;

SELECT TOP 20 ii.Id, ii.WarehouseId, w.Name as WarehouseName, ii.ProductId, p.Sku, ii.QuantityOnHand 
FROM dbo.InventoryItems ii
JOIN dbo.Warehouses w ON ii.WarehouseId = w.Id
JOIN dbo.Products p ON ii.ProductId = p.Id
WHERE ii.ProductId IN (SELECT TOP 5 ProductId FROM dbo.OrderItems);
