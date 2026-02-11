namespace CWI.Application.DTOs.Products;

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Temel Özellikler
    public string BrandName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    
    // Fiyat ve Stok
    public decimal PurchasePrice { get; set; }
    public int StockCount { get; set; }
    public bool IsInStock { get; set; }
    
    // Görselller
    public List<string> Images { get; set; } = new();
    
    // Ek Özellikler (JSON'dan parse edilmiş)
    public Dictionary<string, string> Attributes { get; set; } = new();
}
