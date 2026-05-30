using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ism bo'sh bo'lishi mumkin emas.")
            .MinimumLength(2).WithMessage("Ism kamida 2 ta harf bo'lishi kerak.")
            .MaximumLength(50).WithMessage("Ism 50 ta harfdan oshmasligi kerak.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Familiya bo'sh bo'lishi mumkin emas.")
            .MinimumLength(2).WithMessage("Familiya kamida 2 ta harf bo'lishi kerak.")
            .MaximumLength(50).WithMessage("Familiya 50 ta harfdan oshmasligi kerak.");

        RuleFor(x => x.Age)
            .InclusiveBetween(16, 100).WithMessage("Yosh 16 dan 100 gacha bo'lishi kerak.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon raqam bo'sh bo'lishi mumkin emas.")
            .Matches(@"^\+998\d{9}$").WithMessage("Telefon raqam formati: +998XXXXXXXXX");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parol bo'sh bo'lishi mumkin emas.")
            .MinimumLength(8).WithMessage("Parol kamida 8 ta belgi bo'lishi kerak.")
            .Matches(@"[A-Z]").WithMessage("Parolda kamida 1 ta katta harf bo'lishi kerak.")
            .Matches(@"[0-9]").WithMessage("Parolda kamida 1 ta raqam bo'lishi kerak.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Noto'g'ri rol kiritildi.");
    }
}