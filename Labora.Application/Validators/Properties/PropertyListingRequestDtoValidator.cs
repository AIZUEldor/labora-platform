using FluentValidation;
using Labora.Application.DTOs.Properties;
using Labora.Domain.Enums;

namespace Labora.Application.Validators.Properties;

public class PropertyListingRequestDtoValidator : AbstractValidator<PropertyListingRequestDto>
{
    public PropertyListingRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Sarlavha bo'sh bo'lishi mumkin emas.")
            .MaximumLength(120).WithMessage("Sarlavha 120 ta belgidan oshmasligi kerak.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Tavsif bo'sh bo'lishi mumkin emas.")
            .MaximumLength(3000).WithMessage("Tavsif 3000 ta belgidan oshmasligi kerak.");

        RuleFor(x => x.PropertyType)
            .IsInEnum().WithMessage("Noto'g'ri mulk turi kiritildi.");

        RuleFor(x => x.RoomCount)
            .GreaterThan(0).WithMessage("Xonalar soni 0 dan katta bo'lishi kerak.");

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0).WithMessage("Maydon 0 dan katta bo'lishi kerak.");

        RuleFor(x => x.RenovationStatus)
            .IsInEnum().WithMessage("Noto'g'ri ta'mirlash holati kiritildi.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Narx 0 dan katta bo'lishi kerak.");

        RuleFor(x => x.RentalPeriod)
            .IsInEnum().WithMessage("Noto'g'ri ijara muddati kiritildi.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Manzil bo'sh bo'lishi mumkin emas.")
            .MaximumLength(500).WithMessage("Manzil 500 ta belgidan oshmasligi kerak.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude -90 va 90 oralig'ida bo'lishi kerak.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude -180 va 180 oralig'ida bo'lishi kerak.");

        RuleFor(x => x.ContactPhoneNumber)
            .NotEmpty().WithMessage("Aloqa uchun telefon raqami bo'sh bo'lishi mumkin emas.")
            .MaximumLength(20).WithMessage("Telefon raqami 20 ta belgidan oshmasligi kerak.");

        // Apartment/House are mutually exclusive about floor fields - each pair below only fires
        // for the relevant PropertyType, so the two rules never conflict for a single request.
        RuleFor(x => x.FloorNumber)
            .NotNull().WithMessage("Kvartira uchun qavat raqami kiritilishi shart.")
            .When(x => x.PropertyType == PropertyType.Apartment);

        RuleFor(x => x.TotalFloors)
            .NotNull().WithMessage("Kvartira uchun binoning umumiy qavatlar soni kiritilishi shart.")
            .When(x => x.PropertyType == PropertyType.Apartment);

        RuleFor(x => x.FloorNumber)
            .Null().WithMessage("Uy uchun qavat raqami kiritilmasligi kerak.")
            .When(x => x.PropertyType == PropertyType.House);

        RuleFor(x => x.TotalFloors)
            .Null().WithMessage("Uy uchun binoning umumiy qavatlar soni kiritilmasligi kerak.")
            .When(x => x.PropertyType == PropertyType.House);

        RuleFor(x => x.FloorNumber)
            .GreaterThan(0).WithMessage("Qavat raqami 0 dan katta bo'lishi kerak.")
            .When(x => x.FloorNumber.HasValue);

        RuleFor(x => x.TotalFloors)
            .GreaterThan(0).WithMessage("Binoning umumiy qavatlar soni 0 dan katta bo'lishi kerak.")
            .When(x => x.TotalFloors.HasValue);

        RuleFor(x => x)
            .Must(x => !x.FloorNumber.HasValue || !x.TotalFloors.HasValue || x.FloorNumber.Value <= x.TotalFloors.Value)
            .WithMessage("Qavat raqami binoning umumiy qavatlar sonidan oshmasligi kerak.")
            .WithName("FloorNumber");
    }
}
