namespace CWI.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsInStock { get; set; }
    public int StockCount { get; set; }
}

public class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
