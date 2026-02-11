using System.Text.Json;
using CWI.Application.DTOs.Products;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Products.Queries.GetProductDetail;

public class GetProductDetailQuery : IRequest<ProductDetailDto?>
{
    public int Id { get; set; }

    public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, ProductDetailDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductDetailQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductDetailDto?> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.Repository<Product, int>();

            var product = await productRepo.AsQueryable()
                .AsNoTracking()
                .Include(x => x.Brand)
                .Include(x => x.Color)
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Prices)
                .Include(x => x.Images)
                .Include(x => x.InventoryItems)
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (product == null)
            {
                return null;
            }

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                BrandName = product.Brand?.Name ?? string.Empty,
                ColorName = product.Color?.Name ?? string.Empty,
                CategoryName = product.Category?.Name ?? string.Empty,
                SubCategoryName = product.SubCategory?.Name ?? string.Empty,
                PurchasePrice = product.Prices.OrderByDescending(p => p.ValidFrom).Select(p => p.UnitPrice).FirstOrDefault(),
                StockCount = product.InventoryItems.Sum(i => i.QuantityOnHand - i.QuantityReserved),
                IsInStock = product.InventoryItems.Sum(i => i.QuantityOnHand - i.QuantityReserved) > 0,
                Images = product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
                Description = product.Translations.FirstOrDefault(t => !string.IsNullOrEmpty(t.Description))?.Description ?? string.Empty
            };

            // Parse Attributes JSON
            if (!string.IsNullOrWhiteSpace(product.Attributes))
            {
                try
                {
                    var attributesJson = JsonSerializer.Deserialize<Dictionary<string, string>>(product.Attributes);

                    if (attributesJson != null && attributesJson.Any())
                    {
                        // Get Attribute Definitions for meaningful names
                        var attributeRepo = _unitOfWork.Repository<ProductAttribute, int>();
                        
                        // Kod ve İsim eşleşmelerini çekiyoruz
                        var attributeDefinitions = await attributeRepo.AsQueryable()
                            .AsNoTracking()
                            .Where(a => a.IsActive)
                            .Select(a => new { a.Code, a.Name })
                            .ToDictionaryAsync(a => a.Code, a => a.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

                        var filteredAttributes = new Dictionary<string, string>();

                        foreach (var attr in attributesJson)
                        {
                            // 1. Değer kontrolü: Boşsa gösterme
                            if (string.IsNullOrWhiteSpace(attr.Value))
                            {
                                continue;
                            }

                            string displayKey = attr.Key;
                            bool shouldInclude = true;

                            // 2. İsim Eşleştirme: Veritabanından okunabilir ismini bul
                            if (attributeDefinitions.TryGetValue(attr.Key, out var readableName) && !string.IsNullOrWhiteSpace(readableName))
                            {
                                displayKey = readableName;
                            }


                            if (shouldInclude)
                            {
                                // Eğer aynı isimde özellik varsa (nadiren olabilir), üzerine yaz veya ekle. Dictionary key unique olmalı.
                                if (!filteredAttributes.ContainsKey(displayKey))
                                {
                                    filteredAttributes.Add(displayKey, attr.Value);
                                }
                            }
                        }

                        dto.Attributes = filteredAttributes;
                    }
                }
                catch
                {
                    // JSON parse hatası olursa boş geçiyoruz
                }
            }

            return dto;
        }
    }
}
