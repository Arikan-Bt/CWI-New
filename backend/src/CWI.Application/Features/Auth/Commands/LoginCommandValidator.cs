using FluentValidation;

namespace CWI.Application.Features.Auth.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Request.UserCode)
            .NotEmpty().WithMessage("User code/username cannot be empty.");

        RuleFor(v => v.Request.Password)
            .NotEmpty().WithMessage("Password cannot be empty.");
    }
}
