using FluentValidation;
using Labora.Application.DTOs.Users;

namespace Labora.Application.Validators.Users;

public class ChangePhoneStartRequestDtoValidator : AbstractValidator<ChangePhoneStartRequestDto>
{
    public ChangePhoneStartRequestDtoValidator()
    {
        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("Telefon raqam bo'sh bo'lishi mumkin emas.")
            .Matches(@"^\+998\d{9}$").WithMessage("Telefon raqam formati: +998XXXXXXXXX");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Joriy parol bo'sh bo'lishi mumkin emas.");
    }
}
