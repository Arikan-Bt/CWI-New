using CWI.Application.DTOs.Purchasing;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Products;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace CWI.Application.Features.Purchasing.Commands.CreatePurchaseOrderFromExcel;

public record CreatePurchaseOrderFromExcelCommand(CreatePurchaseOrderFromExcelRequest Request) : IRequest<CreatePurchaseOrderFromExcelResponse>;

public class CreatePurchaseOrderFromExcelCommandHandler : IRequestHandler<CreatePurchaseOrderFromExcelCommand, CreatePurchaseOrderFromExcelResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseOrderFromExcelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePurchaseOrderFromExcelResponse> Handle(CreatePurchaseOrderFromExcelCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            // 0. Ön kontroller
            if (request == null)
            {
                return new CreatePurchaseOrderFromExcelResponse { Message = "Request data could not be read." };
            }

            if (request.File == null || request.File.Length == 0)
            {
                return new CreatePurchaseOrderFromExcelResponse { Message = "No file selected for upload or the file is empty." };
            }

            if (string.IsNullOrWhiteSpace(request.VendorCode))
            {
                return new CreatePurchaseOrderFromExcelResponse { Message = "Please select a vendor." };
            }

            // 1. Dosyayı oku
            using var stream = request.File.OpenReadStream();
            var excelData = stream.Query<PurchaseOrderExcelModel>().ToList();

            Console.WriteLine($"[Excel Import] Row Count: {excelData.Count}");

            // SKU Listesini hazırla (Case-insensitive distinct)
            var skus = excelData
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductCode))
                .Select(x => x.ProductCode.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Console.WriteLine($"[Excel Import] Valid SKU Count: {skus.Count}");
            if (skus.Any())
            {
                Console.WriteLine($"[Excel Import] First 3 SKUs: {string.Join(", ", skus.Take(3))}");
            }

            if (!skus.Any())
            {
                var rowWithData = excelData.FirstOrDefault();
                return new CreatePurchaseOrderFromExcelResponse 
                { 
                    Message = "Could not read product codes from Excel. Ensure headers are 'Product Code' (or 'ProductCode'), 'Quantity', and 'Unit Price'." 
                };
            }

            // 2. Tedarikçiyi bul
            var vendor = await _unitOfWork.Repository<Customer>().FirstOrDefaultAsync(c => c.Code == request.VendorCode, cancellationToken);
            if (vendor == null)
            {
                return new CreatePurchaseOrderFromExcelResponse { Message = $"Vendor not found: {request.VendorCode}" };
            }

            // 3. Ürünleri toplu getir
            var products = await _unitOfWork.Repository<Product>()
                .AsQueryable()
                .Where(p => skus.Contains(p.Sku))
                .ToListAsync(cancellationToken);

            var productMap = products.ToDictionary(p => p.Sku, p => p, StringComparer.OrdinalIgnoreCase);

            var validItems = new List<PurchaseOrderItem>();
            var notFoundSkus = new HashSet<string>();
            decimal totalAmount = 0;
            int totalQuantity = 0;

            // 4. Kalemleri hazırla ve eşleştir
            foreach (var item in excelData)
            {
                if (string.IsNullOrWhiteSpace(item.ProductCode)) continue;
                
                var sku = item.ProductCode.Trim();

                if (!productMap.TryGetValue(sku, out var product))
                {
                    notFoundSkus.Add(sku);
                    continue;
                }

                var orderItem = new PurchaseOrderItem
                {
                    ProductId = product.Id,
                    ProductCode = product.Sku,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    ReceivedQuantity = 0,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice
                };

                validItems.Add(orderItem);
                
                totalAmount += orderItem.LineTotal;
                totalQuantity += orderItem.Quantity;
            }

            if (!validItems.Any())
            {
                 var msg = "No matching products were found.";
                 if (notFoundSkus.Any()) 
                 {
                    msg += $" Missing SKUs: {string.Join(", ", notFoundSkus.Take(5))}{(notFoundSkus.Count > 5 ? "..." : "")}";
                 }
                 return new CreatePurchaseOrderFromExcelResponse { Message = msg };
            }

            // 5. Sipariş numarasını ve belge numarasını oluştur
            var todayStr = DateTime.Now.ToString("yyyyMMdd");
            var lastOrder = await _unitOfWork.Repository<PurchaseOrder, long>()
                .AsQueryable()
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync(cancellationToken);

            int nextDocNumber = (lastOrder?.DocumentNumber ?? 0) + 1;
            
            var dailyCount = await _unitOfWork.Repository<PurchaseOrder, long>()
                .AsQueryable()
                .CountAsync(o => o.OrderNumber.StartsWith($"PO-{todayStr}"), cancellationToken);
            
            var orderNumber = $"PO-{todayStr}-{(dailyCount + 1):D3}";

            // 6. Ana sipariş kaydını oluştur
            var purchaseOrder = new PurchaseOrder
            {
                OrderNumber = orderNumber,
                DocumentNumber = nextDocNumber,
                OrderedAt = request.OrderDate,
                DeliveryDate = request.DeliveryDate,
                SupplierId = vendor.Id,
                SupplierName = vendor.Name,
                ExternalReference = request.Description,
                IsReceived = false,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = totalAmount,
                TotalQuantity = totalQuantity
            };

            await _unitOfWork.Repository<PurchaseOrder, long>().AddAsync(purchaseOrder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Kalemleri kaydet
            var itemRepo = _unitOfWork.Repository<PurchaseOrderItem, long>();
            
            foreach (var item in validItems)
            {
                item.PurchaseOrderId = purchaseOrder.Id;
                await itemRepo.AddAsync(item, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreatePurchaseOrderFromExcelResponse
            {
                Id = purchaseOrder.Id,
                OrderNumber = purchaseOrder.OrderNumber,
                ProcessedItemsCount = validItems.Count,
                Message = $"Purchase order created successfully. Order No: {orderNumber}, Item Count: {validItems.Count}"
            };
        }
        catch (Exception ex)
        {
            return new CreatePurchaseOrderFromExcelResponse { Message = $"An error occurred during processing: {ex.Message}" };
        }
    }
}
