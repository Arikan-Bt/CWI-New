using FluentValidation;

namespace CWI.Application.Features.Inventory.Commands.DeleteWarehouse;

/// <summary>
/// DeleteWarehouseCommand validator
/// </summary>
public class DeleteWarehouseCommandValidator : AbstractValidator<DeleteWarehouseCommand>
{
    public DeleteWarehouseCommandValidator()
    {
        // ID zorunlu ve pozitif
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Warehouse ID must be greater than 0.");
    }
}
