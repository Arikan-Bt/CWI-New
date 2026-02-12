using CWI.Application.DTOs.Payments;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Enums;
using CWI.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CWI.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentUserService _currentUserService;
    
    // Geçerli dosya formatları - Daha sonra konfigürasyona taşınabilir
    private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

    public CreatePaymentCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
        _currentUserService = currentUserService;
    }

    public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 0. Müşteriyi bul
            var customer = await _unitOfWork.Repository<Customer, int>()
                .FirstOrDefaultAsync(x => x.Code == request.CustomerCode, cancellationToken);

            if (customer == null)
            {
                return new CreatePaymentResponse { Success = false, Message = "Invalid customer code." };
            }

            // 1. Para birimini bul
            var normalizedCurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
                ? "USD"
                : request.CurrencyCode.Trim().ToUpperInvariant();

            var currency = await _unitOfWork.Repository<Currency, int>()
                .FirstOrDefaultAsync(x => x.Code == normalizedCurrencyCode, cancellationToken);
            
            if (currency == null)
            {
                return new CreatePaymentResponse { Success = false, Message = "Invalid currency." };
            }

            // 2. Ödeme yöntemini bul (gönderilen kod geçersizse varsayılan yönteme düş)
            var paymentMethodRepo = _unitOfWork.Repository<PaymentMethod, int>();
            PaymentMethod? paymentMethod = null;
            if (!string.IsNullOrWhiteSpace(request.PaymentMethodCode))
            {
                paymentMethod = await paymentMethodRepo
                    .FirstOrDefaultAsync(x => x.Code == request.PaymentMethodCode, cancellationToken);
            }

            paymentMethod ??= await paymentMethodRepo
                .FirstOrDefaultAsync(x => x.IsActive, cancellationToken)
                ?? await paymentMethodRepo.AsQueryable().FirstOrDefaultAsync(cancellationToken);

            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod
                {
                    Code = "MANUAL",
                    Name = "Manual Payment",
                    Description = "Auto-created fallback payment method",
                    SortOrder = 999,
                    IsActive = true
                };

                await paymentMethodRepo.AddAsync(paymentMethod, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 3. Dosyayı kaydet
            string? filePath = null;
            if (request.ReceiptFile != null && request.ReceiptFile.Length > 0)
            {
                var extension = Path.GetExtension(request.ReceiptFile.FileName).ToLowerInvariant();
                
                if (!_allowedExtensions.Contains(extension))
                {
                    return new CreatePaymentResponse 
                    { 
                        Success = false, 
                        Message = $"Invalid file format. Allowed extensions: {string.Join(", ", _allowedExtensions)}" 
                    };
                }

                var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "receipts");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                filePath = Path.Combine("uploads", "receipts", fileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await request.ReceiptFile.CopyToAsync(stream, cancellationToken);
                }
            }

            // 4. Payment entity oluştur
            var payment = new Payment
            {
                CustomerId = customer.Id,
                Amount = request.Amount,
                CurrencyId = currency.Id,
                PaymentMethodId = paymentMethod.Id,
                ReceiptNumber = request.ReceiptNumber,
                PaidAt = request.PaymentDate,
                Notes = request.Notes,
                ReceiptFilePath = filePath,
                Status = PaymentStatus.Pending,
                CreatedByUsername = _currentUserService.UserName ?? "system"
            };

            await _unitOfWork.Repository<Payment, long>().AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Payment kaydedildikten sonra CustomerTransaction oluştur (Credit olarak)
            // Bu kayıt müşterinin cari hareketlerinde görünecek
            var transaction = new CustomerTransaction
            {
                CustomerId = customer.Id,
                TransactionType = TransactionType.Payment,
                TransactionDate = request.PaymentDate,
                ReferenceNumber = request.ReferenceCode ?? request.ReceiptNumber ?? $"PAY-{payment.Id}",
                Description = $"Payment Received - {request.Notes}",
                DocumentType = "Payment",
                ApplicationReference = payment.Id.ToString(),
                DebitAmount = 0,
                CreditAmount = request.Amount,
                Balance = -request.Amount, // Credit olduğu için negatif
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CustomerTransaction, long>().AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreatePaymentResponse 
            { 
                Success = true, 
                PaymentId = payment.Id, 
                Message = "Payment notification saved successfully." 
            };
        }
        catch (Exception ex)
        {
            return new CreatePaymentResponse 
            { 
                Success = false, 
                Message = $"An error occurred while saving the payment: {ex.Message}" 
            };
        }
    }
}
