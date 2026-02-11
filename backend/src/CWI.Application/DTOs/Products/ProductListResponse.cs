namespace CWI.Application.DTOs.Products;

public class ProductListResponse
{
    public List<ProductDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}
