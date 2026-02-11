using FluentValidation;

namespace CWI.Application.Features.Inventory.Commands.CreateWarehouse;

/// <summary>
/// CreateWarehouseCommand validator
/// </summary>
public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
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
