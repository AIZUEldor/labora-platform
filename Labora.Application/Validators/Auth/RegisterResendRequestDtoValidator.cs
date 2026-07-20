using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class RegisterResendRequestDtoValidator : AbstractValidator<RegisterResendRequestDto>
{
    public RegisterResendRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");
    }
}
