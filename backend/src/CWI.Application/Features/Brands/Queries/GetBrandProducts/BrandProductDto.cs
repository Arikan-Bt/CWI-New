namespace CWI.Application.Features.Brands.Queries.GetBrandProducts;

public class BrandProductDto
{
    public int Id { get; set; }
    
    // BrandId null ise false, değilse true (frontend'de checkbox için)
    public bool IsSelected { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? BrandName { get; set; }
}
