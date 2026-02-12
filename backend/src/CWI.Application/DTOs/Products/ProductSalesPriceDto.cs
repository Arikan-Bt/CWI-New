namespace CWI.Application.DTOs.Products;

public class ProductSalesPriceDto
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public bool IsActive { get; set; }
}
