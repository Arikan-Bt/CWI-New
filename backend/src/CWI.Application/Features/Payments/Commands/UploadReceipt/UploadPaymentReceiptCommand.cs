using CWI.Application.DTOs.Payments;
using MediatR;
using Microsoft.AspNetCore.Http; // IFormFile
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Payments;
using Microsoft.AspNetCore.Hosting;
using CWI.Domain.Enums;

namespace CWI.Application.Features.Payments.Commands.UploadReceipt;

public class UploadPaymentReceiptCommand : IRequest<CreatePaymentResponse> // Response tipi aynı olabilir (Success/Message için)
{
    public long PaymentId { get; set; }
    public IFormFile File { get; set; } = null!;

    public class UploadPaymentReceiptHandler : IRequestHandler<UploadPaymentReceiptCommand, CreatePaymentResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

        public UploadPaymentReceiptHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task<CreatePaymentResponse> Handle(UploadPaymentReceiptCommand request, CancellationToken cancellationToken)
        {
            var paymentRepo = _unitOfWork.Repository<Payment, long>();
            var payment = await paymentRepo.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
                return new CreatePaymentResponse { Success = false, Message = "Payment not found." };

            if (request.File == null || request.File.Length == 0)
                return new CreatePaymentResponse { Success = false, Message = "No file was uploaded." };

            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return new CreatePaymentResponse { Success = false, Message = "Invalid file format." };

            var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "receipts");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);
            var relativePath = Path.Combine("uploads", "receipts", fileName).Replace("\\", "/");

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            payment.ReceiptFilePath = relativePath;
            paymentRepo.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreatePaymentResponse { Success = true, Message = "Receipt uploaded successfully." };
        }
    }
}
