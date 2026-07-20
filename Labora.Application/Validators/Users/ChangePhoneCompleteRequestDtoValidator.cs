using FluentValidation;
using Labora.Application.DTOs.Users;

namespace Labora.Application.Validators.Users;

public class ChangePhoneCompleteRequestDtoValidator : AbstractValidator<ChangePhoneCompleteRequestDto>
{
    public ChangePhoneCompleteRequestDtoValidator()
    {
        RuleFor(x => x.VerificationId)
            .NotEqual(Guid.Empty).WithMessage("VerificationId noto'g'ri.");

        RuleFor(x => x.OperationToken)
            .NotEmpty().WithMessage("OperationToken bo'sh bo'lishi mumkin emas.");
    }
}
