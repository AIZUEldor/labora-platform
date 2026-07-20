using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class LoginResendRequestDtoValidator : AbstractValidator<LoginResendRequestDto>
{
    public LoginResendRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");
    }
}
