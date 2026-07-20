using FluentValidation;
using Labora.Application.DTOs.Auth;

namespace Labora.Application.Validators.Auth;

public class RegisterCompleteRequestDtoValidator : AbstractValidator<RegisterCompleteRequestDto>
{
    public RegisterCompleteRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");

        RuleFor(x => x.OperationToken)
            .NotEmpty().WithMessage("OperationToken bo'sh bo'lishi mumkin emas.");
    }
}
