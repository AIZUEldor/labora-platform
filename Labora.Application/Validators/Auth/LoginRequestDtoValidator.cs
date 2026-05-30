using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon raqam bo'sh bo'lishi mumkin emas.")
            .Matches(@"^\+998\d{9}$").WithMessage("Telefon raqam formati: +998XXXXXXXXX");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parol bo'sh bo'lishi mumkin emas.")
            .MinimumLength(8).WithMessage("Parol kamida 8 ta belgi bo'lishi kerak.");
    }
}