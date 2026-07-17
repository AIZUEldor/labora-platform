using FluentValidation;
using Labora.Application.DTOs.Otp;

namespace Labora.Application.Validators.Auth;

public class ForgotPasswordCompleteRequestDtoValidator : AbstractValidator<ForgotPasswordCompleteRequestDto>
{
    public ForgotPasswordCompleteRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");

        RuleFor(x => x.OperationToken)
            .NotEmpty().WithMessage("OperationToken bo'sh bo'lishi mumkin emas.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Parol bo'sh bo'lishi mumkin emas.")
            .MinimumLength(8).WithMessage("Parol kamida 8 ta belgi bo'lishi kerak.")
            .Matches(@"[A-Z]").WithMessage("Parolda kamida 1 ta katta harf bo'lishi kerak.")
            .Matches(@"[0-9]").WithMessage("Parolda kamida 1 ta raqam bo'lishi kerak.");
    }
}
