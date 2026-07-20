using FluentValidation;
using Labora.Application.Common.Validation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class RegisterStartRequestDtoValidator : AbstractValidator<RegisterStartRequestDto>
{
    public RegisterStartRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ism bo'sh bo'lishi mumkin emas.")
            .MinimumLength(RegistrationRules.MinNameLength).WithMessage($"Ism kamida {RegistrationRules.MinNameLength} ta harf bo'lishi kerak.")
            .MaximumLength(RegistrationRules.MaxNameLength).WithMessage($"Ism {RegistrationRules.MaxNameLength} ta harfdan oshmasligi kerak.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Familiya bo'sh bo'lishi mumkin emas.")
            .MinimumLength(RegistrationRules.MinNameLength).WithMessage($"Familiya kamida {RegistrationRules.MinNameLength} ta harf bo'lishi kerak.")
            .MaximumLength(RegistrationRules.MaxNameLength).WithMessage($"Familiya {RegistrationRules.MaxNameLength} ta harfdan oshmasligi kerak.");

        RuleFor(x => x.Age)
            .InclusiveBetween(RegistrationRules.MinAge, RegistrationRules.MaxAge)
            .WithMessage($"Yosh {RegistrationRules.MinAge} dan {RegistrationRules.MaxAge} gacha bo'lishi kerak.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon raqam bo'sh bo'lishi mumkin emas.")
            .Matches(@"^\+998\d{9}$").WithMessage("Telefon raqam formati: +998XXXXXXXXX");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parol bo'sh bo'lishi mumkin emas.")
            .MinimumLength(8).WithMessage("Parol kamida 8 ta belgi bo'lishi kerak.")
            .Matches(@"[A-Z]").WithMessage("Parolda kamida 1 ta katta harf bo'lishi kerak.")
            .Matches(@"[0-9]").WithMessage("Parolda kamida 1 ta raqam bo'lishi kerak.");

        RuleFor(x => x.Role)
            .Must(RegistrationRules.IsAllowedPublicRole).WithMessage("Noto'g'ri rol kiritildi.");
    }
}
