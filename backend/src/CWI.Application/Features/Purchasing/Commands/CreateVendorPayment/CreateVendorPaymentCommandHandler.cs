using CWI.Application.DTOs.Purchasing;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Entities.Payments; // For Currency
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CWI.Application.Features.Purchasing.Commands.CreateVendorPayment;

public class CreateVendorPaymentCommandHandler : IRequestHandler<CreateVendorPaymentCommand, CreateVendorPaymentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    
    private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

    public CreateVendorPaymentCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<CreateVendorPaymentResponse> Handle(CreateVendorPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 0. Tedarikçiyi bul
            var vendor = await _unitOfWork.Repository<Customer, int>()
                .FirstOrDefaultAsync(x => x.Code == request.VendorCode, cancellationToken);

            if (vendor == null)
            {
                return new CreateVendorPaymentResponse { Success = false, Message = "Invalid vendor code." };
            }

            // 1. Para birimini bul
            var normalizedCurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
                ? "USD"
                : request.CurrencyCode.Trim().ToUpperInvariant();

            var currency = await _unitOfWork.Repository<Currency, int>()
                .FirstOrDefaultAsync(x => x.Code == normalizedCurrencyCode, cancellationToken);
            
            if (currency == null)
            {
                return new CreateVendorPaymentResponse { Success = false, Message = "Invalid currency." };
            }

            // 1.5. Faturayı bul (ReferenceNumber = InvoiceNumber varsayımıyla)
            var invoice = await _unitOfWork.Repository<VendorInvoice, int>()
                .FirstOrDefaultAsync(x => x.InvoiceNumber == request.ReferenceNumber && x.VendorId == vendor.Id, cancellationToken);

            // 2. Dosyayı kaydet
            string? filePath = null;
            if (request.PaymentFile != null && request.PaymentFile.Length > 0)
            {
                var extension = Path.GetExtension(request.PaymentFile.FileName).ToLowerInvariant();
                
                if (!_allowedExtensions.Contains(extension))
                {
                    return new CreateVendorPaymentResponse 
                    { 
                        Success = false, 
                        Message = $"Invalid file format. Allowed extensions: {string.Join(", ", _allowedExtensions)}" 
                    };
                }

                var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "vendor-payments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                filePath = Path.Combine("uploads", "vendor-payments", fileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await request.PaymentFile.CopyToAsync(stream, cancellationToken);
                }
            }

            // 3. VendorPayment entity oluştur
            var payment = new VendorPayment
            {
                VendorId = vendor.Id,
                Amount = request.Amount,
                CurrencyId = currency.Id,
                PaidAt = request.PaymentDate,
                ReferenceNumber = request.ReferenceNumber,
                Description = request.Description,
                FilePath = filePath,
                VendorInvoiceId = invoice?.Id
            };

            await _unitOfWork.Repository<VendorPayment, long>().AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateVendorPaymentResponse 
            { 
                Success = true, 
                Id = payment.Id,
                Message = "Payment notification saved successfully." 
            };
        }
        catch (Exception ex)
        {
            return new CreateVendorPaymentResponse 
            { 
                Success = false, 
                Message = $"An error occurred while saving the payment: {ex.Message}" 
            };
        }
    }
}
