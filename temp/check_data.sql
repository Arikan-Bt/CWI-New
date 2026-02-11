
SELECT Id, Code, Name FROM dbo.Warehouses;
SELECT TOP 5 OrderId, ProductId, Qty, Price FROM dbo.OrderItems;
SELECT TOP 5 p.Id, p.Code, p.Name, oi.OrderId FROM dbo.Products p JOIN dbo.OrderItems oi ON p.Id = oi.ProductId;
SELECT * FROM dbo.InventoryItems WHERE ProductId IN (SELECT TOP 5 ProductId FROM dbo.OrderItems);
