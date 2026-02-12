using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Entities.Products;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Globalization;

namespace CWI.Application.Features.Import.Commands.ImportOrder;

public class ImportOrderCommand : IRequest<ImportOrderResponse>
{
    public string FileContent { get; set; } = string.Empty;
    public string ProjectType { get; set; } = "CWI";
    public string CustomerCode { get; set; } = string.Empty;
    public string OrderType { get; set; } = "Order";
}

public class ImportOrderResponse
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ImportErrorDetailDto> Errors { get; set; } = new();
}

public class ImportErrorDetailDto
{
    public int Row { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ImportOrderCommandHandler : IRequestHandler<ImportOrderCommand, ImportOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ImportOrderCommandHandler> _logger;

    public ImportOrderCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ImportOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ImportOrderResponse> Handle(ImportOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ImportOrderCommand started. CustomerCode: {CustomerCode}, ProjectType: {ProjectType}, FileLength: {FileLength}",
            request.CustomerCode,
            request.ProjectType,
            request.FileContent.Length);

        var response = new ImportOrderResponse();

        try
        {
            var bytes = Convert.FromBase64String(
                request.FileContent.Contains(",")
                    ? request.FileContent.Split(',')[1]
                    : request.FileContent);

            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;
            response.TotalRows = rowCount - 1;

            _logger.LogInformation("Excel file parsed. Total rows (excluding header): {TotalRows}", response.TotalRows);

            var excelData = new List<ExcelRowData>();

            // 1) Read and validate rows from excel
            for (int row = 2; row <= rowCount; row++)
            {
                var productCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var qtyStr = worksheet.Cells[row, 2].Value?.ToString();
                var priceStr = worksheet.Cells[row, 3].Value?.ToString();
                var season = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(productCode) || !int.TryParse(qtyStr, out var qty) || qty <= 0)
                {
                    _logger.LogWarning(
                        "Row {Row}: Missing or invalid data. ProductCode: {ProductCode}, Qty: {QtyStr}",
                        row,
                        productCode,
                        qtyStr);
                    response.Errors.Add(new ImportErrorDetailDto { Row = row, Message = "Missing or invalid data." });
                    continue;
                }

                excelData.Add(new ExcelRowData
                {
                    Row = row,
                    ProductCode = productCode,
                    Quantity = qty,
                    Price = ParsePrice(priceStr),
                    Season = season
                });
            }

            // 2) Customer
            var customer = await _unitOfWork.Repository<Customer, int>()
                .AsQueryable()
                .FirstOrDefaultAsync(c => c.Code == request.CustomerCode, cancellationToken);

            if (customer == null)
            {
                _logger.LogError("Customer not found. Code: {CustomerCode}", request.CustomerCode);
                response.Errors.Add(new ImportErrorDetailDto { Row = 0, Message = $"Customer not found: {request.CustomerCode}" });
                response.ErrorCount = response.Errors.Count;
                return response;
            }

            if (excelData.Count == 0)
            {
                _logger.LogWarning("No valid data found in Excel to import.");
                response.Errors.Add(new ImportErrorDetailDto { Row = 0, Message = "No valid rows found to import." });
                response.ErrorCount = response.Errors.Count;
                return response;
            }

            // 3) Currency (auto-create fallback when table is empty)
            var currency = await EnsureDefaultCurrencyAsync(cancellationToken);

            // 4) Warehouse
            var warehouse = await _unitOfWork.Repository<Warehouse>()
                .FirstOrDefaultAsync(x => x.IsDefault, cancellationToken)
                ?? await _unitOfWork.Repository<Warehouse>().FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

            if (warehouse == null)
            {
                _logger.LogError("Default warehouse not found.");
                response.Errors.Add(new ImportErrorDetailDto { Row = 0, Message = "No warehouse is configured in the system." });
                response.ErrorCount = response.Errors.Count;
                return response;
            }

            // 5) Product match (trim + case-insensitive)
            var normalizedSkuSet = excelData
                .Select(x => NormalizeSku(x.ProductCode))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var products = await _unitOfWork.Repository<Product, int>()
                .AsQueryable()
                .Include(p => p.Prices)
                .Where(p => normalizedSkuSet.Contains(((p.Sku ?? string.Empty).Trim().ToUpper())))
                .ToListAsync(cancellationToken);

            var productBySku = products
                .GroupBy(p => NormalizeSku(p.Sku))
                .ToDictionary(g => g.Key, g => g.First());

            var matchedRows = new List<(ExcelRowData Row, Product Product)>();

            foreach (var item in excelData)
            {
                var normalizedSku = NormalizeSku(item.ProductCode);

                if (!productBySku.TryGetValue(normalizedSku, out var product))
                {
                    _logger.LogWarning("Row {Row}: Product not found. SKU: {ProductCode}", item.Row, item.ProductCode);
                    response.Errors.Add(new ImportErrorDetailDto { Row = item.Row, Message = $"Product not found: {item.ProductCode}" });
                    continue;
                }

                matchedRows.Add((item, product));
            }

            if (matchedRows.Count == 0)
            {
                _logger.LogWarning("No valid products matched for import. CustomerCode: {CustomerCode}", request.CustomerCode);
                response.Errors.Add(new ImportErrorDetailDto
                {
                    Row = 0,
                    Message = "None of the product codes in the Excel file exist in the system. Order was not created.",
                });
                response.ErrorCount = response.Errors.Count;
                return response;
            }

            // 6) Create order only if at least one valid line exists
            var isPreOrder = string.Equals(request.OrderType, "PreOrder", StringComparison.OrdinalIgnoreCase);
            var isShipped = string.Equals(request.OrderType, "Shipped", StringComparison.OrdinalIgnoreCase);

            var matchedGroups = matchedRows
                .GroupBy(x => x.Product.Id)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductCode = g.First().Product.Sku,
                    TotalQty = g.Sum(x => x.Row.Quantity)
                })
                .ToList();

            var productIds = matchedGroups.Select(x => x.ProductId).ToList();

            var inventoryByProduct = await _unitOfWork.Repository<InventoryItem, long>()
                .AsQueryable()
                .Where(i => i.WarehouseId == warehouse.Id && productIds.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId, cancellationToken);

            var incomingByProduct = await _unitOfWork.Repository<PurchaseOrderItem, long>()
                .AsQueryable()
                .Where(x => productIds.Contains(x.ProductId) && x.Quantity > x.ReceivedQuantity)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    IncomingQuantity = g.Sum(x => x.Quantity - x.ReceivedQuantity)
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.IncomingQuantity, cancellationToken);

            foreach (var g in matchedGroups)
            {
                inventoryByProduct.TryGetValue(g.ProductId, out var inv);
                var onHand = inv?.QuantityOnHand ?? 0;
                var reserved = inv?.QuantityReserved ?? 0;
                var available = onHand - reserved;
                var incoming = incomingByProduct.TryGetValue(g.ProductId, out var incQty) ? incQty : 0;
                var preorderCapacity = onHand + incoming;

                if (isPreOrder)
                {
                    if (g.TotalQty > preorderCapacity)
                    {
                        response.Errors.Add(new ImportErrorDetailDto
                        {
                            Row = 0,
                            Message = $"Insufficient stock for PreOrder. Product: {g.ProductCode}, Requested: {g.TotalQty}, OnHand+Incoming: {preorderCapacity}"
                        });
                    }
                }
                else
                {
                    if (g.TotalQty > available)
                    {
                        response.Errors.Add(new ImportErrorDetailDto
                        {
                            Row = 0,
                            Message = $"Insufficient available stock. Product: {g.ProductCode}, Requested: {g.TotalQty}, Available: {available}"
                        });
                    }
                }
            }

            if (response.Errors.Count > 0)
            {
                response.ErrorCount = response.Errors.Count;
                return response;
            }

            var orderNumber = $"IMP-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderNumber = orderNumber,
                OrderedAt = DateTime.UtcNow,
                Status = isPreOrder
                    ? OrderStatus.PreOrder
                    : (isShipped ? OrderStatus.Shipped : OrderStatus.Pending),
                IsPreOrder = isPreOrder,
                CreatedByGroupCode = request.ProjectType,
                Notes = "Created via Excel import",
                CreatedAt = DateTime.UtcNow,
                CreatedByUsername = _currentUserService.UserName ?? "system",
                CurrencyId = currency.Id,
                Season = excelData.FirstOrDefault(x => !string.IsNullOrEmpty(x.Season))?.Season
            };

            await _unitOfWork.Repository<Order, long>().AddAsync(order, cancellationToken);
            var stockMovementRepo = _unitOfWork.Repository<StockMovement, long>();
            _logger.LogInformation("Order entity created (pre-save). OrderNumber: {OrderNumber}", orderNumber);

            decimal subTotal = 0;
            int totalQty = 0;

            foreach (var matched in matchedRows)
            {
                var item = matched.Row;
                var product = matched.Product;
                var fallbackPrice = product.Prices
                    .OrderByDescending(p => p.ValidFrom)
                    .FirstOrDefault(p => p.IsActive)?.UnitPrice ?? 0;
                var unitPrice = item.Price ?? fallbackPrice;

                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = unitPrice * item.Quantity,
                    WarehouseId = warehouse.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUsername = _currentUserService.UserName ?? "system",
                };

                await _unitOfWork.Repository<OrderItem, long>().AddAsync(orderItem, cancellationToken);

                var inventoryItem = await _unitOfWork.Repository<InventoryItem, long>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.WarehouseId == warehouse.Id, cancellationToken);

                if (inventoryItem == null)
                {
                    var beforeOnHand = 0;
                    var beforeReserved = 0;
                    inventoryItem = new InventoryItem
                    {
                        ProductId = product.Id,
                        WarehouseId = warehouse.Id,
                        QuantityOnHand = 0,
                        QuantityReserved = isShipped ? 0 : item.Quantity,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    if (isShipped)
                    {
                        inventoryItem.QuantityOnHand -= item.Quantity;
                    }

                    await _unitOfWork.Repository<InventoryItem, long>().AddAsync(inventoryItem, cancellationToken);
                    await stockMovementRepo.AddAsync(new StockMovement
                    {
                        ProductId = product.Id,
                        WarehouseId = warehouse.Id,
                        MovementType = isShipped ? StockMovementType.Sale : StockMovementType.Reserve,
                        QuantityDeltaOnHand = isShipped ? -item.Quantity : 0,
                        QuantityDeltaReserved = isShipped ? 0 : item.Quantity,
                        BeforeOnHand = beforeOnHand,
                        AfterOnHand = inventoryItem.QuantityOnHand,
                        BeforeReserved = beforeReserved,
                        AfterReserved = inventoryItem.QuantityReserved,
                        SourceDocumentType = "ImportedSalesOrder",
                        SourceDocumentId = null,
                        ReferenceNo = orderNumber,
                        OccurredAt = DateTime.UtcNow,
                        Description = $"Import order stock impact ({request.OrderType})"
                    }, cancellationToken);
                    _logger.LogInformation(
                        "Created new InventoryItem for ProductId: {ProductId} in WarehouseId: {WarehouseId}. OrderType: {OrderType}, Qty: {Quantity}",
                        product.Id,
                        warehouse.Id,
                        request.OrderType,
                        item.Quantity);
                }
                else
                {
                    var beforeOnHand = inventoryItem.QuantityOnHand;
                    var beforeReserved = inventoryItem.QuantityReserved;
                    if (isShipped)
                    {
                        inventoryItem.QuantityOnHand -= item.Quantity;
                    }
                    else
                    {
                        inventoryItem.QuantityReserved += item.Quantity;
                    }

                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<InventoryItem, long>().Update(inventoryItem);
                    await stockMovementRepo.AddAsync(new StockMovement
                    {
                        ProductId = product.Id,
                        WarehouseId = warehouse.Id,
                        MovementType = isShipped ? StockMovementType.Sale : StockMovementType.Reserve,
                        QuantityDeltaOnHand = isShipped ? -item.Quantity : 0,
                        QuantityDeltaReserved = isShipped ? 0 : item.Quantity,
                        BeforeOnHand = beforeOnHand,
                        AfterOnHand = inventoryItem.QuantityOnHand,
                        BeforeReserved = beforeReserved,
                        AfterReserved = inventoryItem.QuantityReserved,
                        SourceDocumentType = "ImportedSalesOrder",
                        SourceDocumentId = null,
                        ReferenceNo = orderNumber,
                        OccurredAt = DateTime.UtcNow,
                        Description = $"Import order stock impact ({request.OrderType})"
                    }, cancellationToken);
                    _logger.LogInformation(
                        "Updated stock for ProductId: {ProductId}. OrderType: {OrderType}, Qty: {Quantity}, OnHand: {OnHand}, Reserved: {Reserved}",
                        product.Id,
                        request.OrderType,
                        item.Quantity,
                        inventoryItem.QuantityOnHand,
                        inventoryItem.QuantityReserved);
                }

                subTotal += orderItem.LineTotal;
                totalQty += orderItem.Quantity;
            }

            order.SubTotal = subTotal;
            order.GrandTotal = subTotal;
            order.TotalQuantity = totalQty;

            // Keep customer balance references in sync for payment screens
            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
            var existingTransaction = await transactionRepo.FirstOrDefaultAsync(
                t => t.ReferenceNumber == order.OrderNumber && t.CustomerId == order.CustomerId,
                cancellationToken);

            if (existingTransaction == null)
            {
                await transactionRepo.AddAsync(new CustomerTransaction
                {
                    CustomerId = order.CustomerId,
                    TransactionDate = order.OrderedAt,
                    ReferenceNumber = order.OrderNumber,
                    Description = $"Sales Order: {order.OrderNumber}",
                    DocumentType = "SalesOrder",
                    ApplicationReference = order.Id.ToString(),
                    TransactionType = TransactionType.Invoice,
                    DebitAmount = order.GrandTotal,
                    CreditAmount = 0,
                    Balance = order.GrandTotal,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
            else
            {
                existingTransaction.TransactionDate = order.OrderedAt;
                existingTransaction.DebitAmount = order.GrandTotal;
                existingTransaction.CreditAmount = 0;
                existingTransaction.Balance = order.GrandTotal;
                existingTransaction.Description = $"Sales Order: {order.OrderNumber}";
                existingTransaction.DocumentType = "SalesOrder";
                existingTransaction.ApplicationReference = order.Id.ToString();
                transactionRepo.Update(existingTransaction);
            }

            response.SuccessCount = matchedRows.Count;

            _logger.LogInformation(
                "Saving changes. OrderId: {OrderId}, TotalItems: {TotalItems}, SubTotal: {SubTotal}",
                order.Id,
                totalQty,
                subTotal);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Changes saved successfully. OrderId: {OrderId}", order.Id);

            response.ErrorCount = response.Errors.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during import. Message: {Message}", ex.Message);
            response.Errors.Add(new ImportErrorDetailDto { Row = 0, Message = "Import failed: " + ex.Message });
            response.ErrorCount = response.Errors.Count;
        }

        return response;
    }

    private class ExcelRowData
    {
        public int Row { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? Season { get; set; }
    }

    private static string NormalizeSku(string? sku)
    {
        return (sku ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static decimal? ParsePrice(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var normalized = raw.Trim().Replace(" ", string.Empty);

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
        {
            return inv;
        }

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var tr))
        {
            return tr;
        }

        // Retry with comma->dot for mixed excel exports
        normalized = normalized.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out inv))
        {
            return inv;
        }

        return null;
    }

    private async Task<Currency> EnsureDefaultCurrencyAsync(CancellationToken cancellationToken)
    {
        var currencyRepo = _unitOfWork.Repository<Currency>();

        var currency = await currencyRepo.FirstOrDefaultAsync(x => x.IsDefault, cancellationToken)
            ?? await currencyRepo.FirstOrDefaultAsync(x => x.Code == "USD", cancellationToken)
            ?? await currencyRepo.FirstOrDefaultAsync(x => x.IsActive, cancellationToken)
            ?? await currencyRepo.AsQueryable().FirstOrDefaultAsync(cancellationToken);

        if (currency != null) return currency;

        var fallbackCurrency = new Currency
        {
            Code = "USD",
            Name = "US Dollar",
            Symbol = "$",
            IsDefault = true,
            IsActive = true
        };

        await currencyRepo.AddAsync(fallbackCurrency, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return fallbackCurrency;
    }
}
