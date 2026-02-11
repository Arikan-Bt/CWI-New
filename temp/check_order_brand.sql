
-- Check Order 887 Brand info
SELECT DISTINCT b.Name as BrandName
FROM dbo.OrderItems oi
JOIN dbo.Products p ON oi.ProductId = p.Id
JOIN dbo.Brands b ON p.BrandId = b.Id
WHERE oi.OrderId = 887;

-- Check Order Status Enum mapping (usually implicit, but good to check if there's a lookup)
-- Also check if status 2 matches what the report is looking for.
