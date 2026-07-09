using FluentValidation;
using Labora.Application.DTOs.Payments;

namespace Labora.Application.Validators.Payments;

public class CreateTopUpRequestDtoValidator : AbstractValidator<CreateTopUpRequestDto>
{
    private const decimal MinTopUpAmount = 1_000m;
    private const decimal MaxTopUpAmount = 10_000_000m;

    public CreateTopUpRequestDtoValidator()
    {
        RuleFor(x => x.Amount)
            .InclusiveBetween(MinTopUpAmount, MaxTopUpAmount)
            .WithMessage($"To'ldirish miqdori {MinTopUpAmount:N0} dan {MaxTopUpAmount:N0} so'mgacha bo'lishi kerak.");

        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Noto'g'ri to'lov provayderi.");
    }
}
