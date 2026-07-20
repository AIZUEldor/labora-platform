using FluentValidation;
using Labora.Application.DTOs.Users;

namespace Labora.Application.Validators.Users;

public class ChangePhoneResendRequestDtoValidator : AbstractValidator<ChangePhoneResendRequestDto>
{
    public ChangePhoneResendRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");
    }
}
