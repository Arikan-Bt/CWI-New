using FluentValidation;

namespace CWI.Application.Features.Inventory.Commands.UpdateWarehouse;

/// <summary>
/// UpdateWarehouseCommand validator
/// </summary>
public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        // ID zorunlu ve pozitif
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Warehouse ID must be greater than 0.");

        // Code zorunlu ve max 50 karakter
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50).WithMessage("Warehouse code cannot exceed 50 characters.");

        // Name zorunlu ve max 200 karakter
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(200).WithMessage("Warehouse name cannot exceed 200 characters.");

        // Address opsiyonel ama max 500 karakter
        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));
    }
}
