using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class ForgotPasswordResendRequestDtoValidator : AbstractValidator<ForgotPasswordResendRequestDto>
{
    public ForgotPasswordResendRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");
    }
}
