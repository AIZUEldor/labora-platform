using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class LoginCompleteRequestDtoValidator : AbstractValidator<LoginCompleteRequestDto>
{
    public LoginCompleteRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");

        RuleFor(x => x.OperationToken)
            .NotEmpty().WithMessage("OperationToken bo'sh bo'lishi mumkin emas.");
    }
}
