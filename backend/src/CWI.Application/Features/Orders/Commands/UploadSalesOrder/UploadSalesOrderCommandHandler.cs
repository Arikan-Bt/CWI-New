using CWI.Application.DTOs.Orders;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Orders.Commands.UploadSalesOrder;

public class UploadSalesOrderCommandHandler : IRequestHandler<UploadSalesOrderCommand, UploadSalesOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public UploadSalesOrderCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<UploadSalesOrderResponse> Handle(UploadSalesOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1) Resolve customer
            var customer = await _unitOfWork.Repository<Customer>()
                .FirstOrDefaultAsync(x => x.Code == request.CustomerCode, cancellationToken);

            if (customer == null)
            {
                return new UploadSalesOrderResponse { Success = false, Message = "Invalid customer code." };
            }

            // 2) Save file
            var extension = Path.GetExtension(request.OrderFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".xlsx", ".xls", ".csv", ".pdf" };

            if (!allowedExtensions.Contains(extension))
            {
                return new UploadSalesOrderResponse { Success = false, Message = "Invalid file format." };
            }

            var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "orders");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine("uploads", "orders", fileName);
            var fullPath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.OrderFile.CopyToAsync(stream, cancellationToken);
            }

            // 3) Resolve currency (auto-create fallback when table is empty)
            var currency = await EnsureDefaultCurrencyAsync(cancellationToken);

            // 4) Create order
            var order = new Order
            {
                OrderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                OrderedAt = DateTime.Now,
                CustomerId = customer.Id,
                CurrencyId = currency.Id,
                IsPreOrder = request.OrderType == "PreOrder",
                Status = request.OrderType == "PreOrder"
                    ? OrderStatus.PreOrder
                    : (request.OrderType == "Shipped" ? OrderStatus.Shipped : OrderStatus.Pending),
                OrderFilePath = filePath,
                CreatedByUsername = "system", // TODO: Get from context
                Notes = $"Uploaded via Sales Order Entry. Type: {request.OrderType}"
            };

            await _unitOfWork.Repository<Order, long>().AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UploadSalesOrderResponse
            {
                Success = true,
                OrderId = order.Id,
                Message = "Sales order uploaded successfully."
            };
        }
        catch (Exception ex)
        {
            return new UploadSalesOrderResponse
            {
                Success = false,
                Message = $"An error occurred while uploading the order: {ex.Message}"
            };
        }
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
