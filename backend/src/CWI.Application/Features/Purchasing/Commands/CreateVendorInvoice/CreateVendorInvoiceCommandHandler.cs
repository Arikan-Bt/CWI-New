using CWI.Application.DTOs.Purchasing;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Entities.Payments; // For Currency
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CWI.Application.Features.Purchasing.Commands.CreateVendorInvoice;

public class CreateVendorInvoiceCommandHandler : IRequestHandler<CreateVendorInvoiceCommand, CreateVendorInvoiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    
    // Geçerli dosya formatları
    private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

    public CreateVendorInvoiceCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<CreateVendorInvoiceResponse> Handle(CreateVendorInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 0. Tedarikçiyi (Vendor) bul
            var vendor = await _unitOfWork.Repository<Customer, int>()
                .FirstOrDefaultAsync(x => x.Code == request.VendorCode, cancellationToken);

            if (vendor == null)
            {
                return new CreateVendorInvoiceResponse { Success = false, Message = "Invalid vendor code." };
            }

            // 1. Para birimini bul
            var normalizedCurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
                ? "USD"
                : request.CurrencyCode.Trim().ToUpperInvariant();

            var currency = await _unitOfWork.Repository<Currency, int>()
                .FirstOrDefaultAsync(x => x.Code == normalizedCurrencyCode, cancellationToken);
            
            if (currency == null)
            {
                return new CreateVendorInvoiceResponse { Success = false, Message = "Invalid currency." };
            }

            // 2. Dosyayı kaydet
            string? filePath = null;
            if (request.InvoiceFile != null && request.InvoiceFile.Length > 0)
            {
                var extension = Path.GetExtension(request.InvoiceFile.FileName).ToLowerInvariant();
                
                if (!_allowedExtensions.Contains(extension))
                {
                    return new CreateVendorInvoiceResponse 
                    { 
                        Success = false, 
                        Message = $"Invalid file format. Allowed extensions: {string.Join(", ", _allowedExtensions)}" 
                    };
                }

                var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "invoices");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                filePath = Path.Combine("uploads", "invoices", fileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await request.InvoiceFile.CopyToAsync(stream, cancellationToken);
                }
            }

            // 3. VendorInvoice entity oluştur
            var invoice = new VendorInvoice
            {
                VendorId = vendor.Id,
                InvoiceNumber = request.InvoiceNumber,
                InvoicedAt = request.InvoiceDate,
                TotalAmount = request.Amount,
                CurrencyId = currency.Id,
                Description = request.Description,
                FilePath = filePath,
                IsPaid = false,
                PaidAmount = 0
            };

            await _unitOfWork.Repository<VendorInvoice, int>().AddAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateVendorInvoiceResponse 
            { 
                Success = true, 
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                TotalAmount = invoice.TotalAmount,
                FilePath = invoice.FilePath,
                Message = "Invoice saved successfully." 
            };
        }
        catch (Exception ex)
        {
            return new CreateVendorInvoiceResponse 
            { 
                Success = false, 
                Message = $"An error occurred while saving the invoice: {ex.Message}" 
            };
        }
    }
}
