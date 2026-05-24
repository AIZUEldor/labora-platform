using FluentValidation;
using Labora.Application.DTOs.Jobs;

namespace Labora.Application.Validators.Jobs;

public class JobRequestDtoValidator : AbstractValidator<JobRequestDto>
{
    public JobRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ish nomi bo'sh bo'lishi mumkin emas.")
            .MinimumLength(3).WithMessage("Ish nomi kamida 3 ta harf bo'lishi kerak.")
            .MaximumLength(100).WithMessage("Ish nomi 100 ta harfdan oshmasligi kerak.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Tavsif bo'sh bo'lishi mumkin emas.")
            .MinimumLength(10).WithMessage("Tavsif kamida 10 ta belgi bo'lishi kerak.")
            .MaximumLength(2000).WithMessage("Tavsif 2000 ta belgidan oshmasligi kerak.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Maosh 0 dan katta bo'lishi kerak.");

        RuleFor(x => x.JobType)
            .IsInEnum().WithMessage("Noto'g'ri ish turi kiritildi.");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Kategoriya nomi bo'sh bo'lishi mumkin emas.")
            .MaximumLength(100).WithMessage("Kategoriya nomi 100 ta harfdan oshmasligi kerak.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Shahar bo'sh bo'lishi mumkin emas.")
            .MaximumLength(100).WithMessage("Shahar nomi 100 ta harfdan oshmasligi kerak.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Mamlakat bo'sh bo'lishi mumkin emas.")
            .MaximumLength(100).WithMessage("Mamlakat nomi 100 ta harfdan oshmasligi kerak.");

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Tajriba yillari 0 dan kichik bo'lishi mumkin emas.")
            .When(x => x.ExperienceYears.HasValue);

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTime.UtcNow).WithMessage("Deadline kelajakdagi sana bo'lishi kerak.")
            .When(x => x.Deadline.HasValue);
    }
}