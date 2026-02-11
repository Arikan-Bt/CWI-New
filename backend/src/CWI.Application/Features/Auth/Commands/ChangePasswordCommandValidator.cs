using FluentValidation;

namespace CWI.Application.Features.Auth.Commands;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.Request.CurrentPassword)
            .NotEmpty().WithMessage("Current password cannot be empty.");

        RuleFor(v => v.Request.NewPassword)
            .NotEmpty().WithMessage("New password cannot be empty.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");

        RuleFor(v => v.Request.ConfirmPassword)
            .Equal(v => v.Request.NewPassword).WithMessage("Passwords do not match.");
    }
}
