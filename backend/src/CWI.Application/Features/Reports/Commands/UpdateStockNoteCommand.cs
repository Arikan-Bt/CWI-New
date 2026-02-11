using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Commands;

public class UpdateStockNoteCommand : IRequest<bool>
{
    public UpdateStockNoteRequest Request { get; set; } = new();
}

public class UpdateStockNoteHandler : IRequestHandler<UpdateStockNoteCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockNoteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateStockNoteCommand command, CancellationToken cancellationToken)
    {
        var productRepo = _unitOfWork.Repository<Product>();
        var product = await productRepo.AsQueryable()
            .FirstOrDefaultAsync(p => p.Sku == command.Request.ItemCode, cancellationToken);

        if (product == null) return false;

        var noteRepo = _unitOfWork.Repository<ProductNote>();
        var existingNote = await noteRepo.AsQueryable()
            .FirstOrDefaultAsync(n => n.ProductId == product.Id, cancellationToken);

        if (existingNote != null)
        {
            existingNote.Content = command.Request.Note ?? string.Empty;
            noteRepo.Update(existingNote);
        }
        else
        {
            await noteRepo.AddAsync(new ProductNote
            {
                ProductId = product.Id,
                Content = command.Request.Note ?? string.Empty,
                CreatedByUsername = "system",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
