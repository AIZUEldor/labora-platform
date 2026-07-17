using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class ForgotPasswordVerifyRequestDtoValidator : AbstractValidator<ForgotPasswordVerifyRequestDto>
{
    public ForgotPasswordVerifyRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kod bo'sh bo'lishi mumkin emas.")
            .Matches(@"^\d{6}$").WithMessage("Kod 6 ta raqamdan iborat bo'lishi kerak.");
    }
}
