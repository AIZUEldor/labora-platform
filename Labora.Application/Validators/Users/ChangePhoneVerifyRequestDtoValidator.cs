using FluentValidation;
using Labora.Application.DTOs.Users;

namespace Labora.Application.Validators.Users;

public class ChangePhoneVerifyRequestDtoValidator : AbstractValidator<ChangePhoneVerifyRequestDto>
{
    public ChangePhoneVerifyRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kod bo'sh bo'lishi mumkin emas.")
            .Matches(@"^\d{6}$").WithMessage("Kod 6 ta raqamdan iborat bo'lishi kerak.");
    }
}
