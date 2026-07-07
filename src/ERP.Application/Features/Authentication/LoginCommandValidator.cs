using ERP.Application.Features.Authentication;
using FluentValidation;

namespace ERP.Application.Features.Authentication;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Must(u => !string.IsNullOrWhiteSpace(u))
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must have at least 3 characters.")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters.")
            .Matches("^[A-Za-z0-9]{3,50}$")
            .WithMessage("Username must contain only alphanumeric characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Must(p => !string.IsNullOrWhiteSpace(p))
            .WithMessage("Password is required.");
    }
}
