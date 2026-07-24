using AutoMapper;
using Labora.Application.DTOs.Applications;
using Labora.Application.DTOs.Categories;
using Labora.Application.DTOs.Jobs;
using Labora.Application.DTOs.Payments;
using Labora.Application.DTOs.Reviews;
using Labora.Application.DTOs.Transactions;
using Labora.Application.DTOs.Users;
using Labora.Domain.Entities;

namespace Labora.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Job mappings
        // CoverImageUrl is the single source of truth for "first image by CreatedAt, then Id": when
        // Images is loaded (detail/update), this derives the cover image from it directly, so the
        // ordering/selection rule is expressed exactly once rather than duplicated per call site.
        // List responses intentionally don't load Images, so this evaluates to null for them and
        // JobService overwrites it with the repository's separately-projected value instead.
        CreateMap<Job, JobResponseDto>()
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.Images
                .OrderBy(i => i.CreatedAt)
                .ThenBy(i => i.Id)
                .Select(i => i.ImageUrl)
                .FirstOrDefault()));
        CreateMap<JobRequestDto, Job>();
        CreateMap<JobImage, JobImageDto>();
        CreateMap<ApplicationRequestDto, JobApplication>();
        CreateMap<JobApplication, ApplicationResponseDto>()
    .ForMember(dest => dest.WorkerName,
        opt => opt.MapFrom(src => src.Worker != null
            ? $"{src.Worker.FirstName} {src.Worker.LastName}"
            : null))
    .ForMember(dest => dest.WorkerCvUrl,
        opt => opt.MapFrom(src => src.Worker != null
            ? src.Worker.CvUrl
            : null))
    .ForMember(dest => dest.WorkerPhone,
        opt => opt.MapFrom(src => src.Worker != null
            ? src.Worker.PhoneNumber
            : null))
    .ForMember(dest => dest.JobTitle,
        opt => opt.MapFrom(src => src.Job != null
            ? src.Job.Title
            : null));

        // User mappings
        CreateMap<User, UserProfileResponseDto>();
        CreateMap<UpdateProfileRequestDto, User>()
    .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
    .ForMember(dest => dest.CvUrl, opt => opt.Ignore())
    .ForMember(dest => dest.Role, opt => opt.Ignore())
    .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    // PhoneNumber is deliberately ignored here: the general profile-update endpoint must never be able
    // to change the phone number (it currently does so with no OTP/ownership verification and no
    // uniqueness check). Old clients may still send this field - it is silently dropped, not rejected.
    // Changing phone number must go through a dedicated OTP-protected flow (not yet implemented).
    .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore());

        // Category mappings
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(dest => dest.SubCategories,
                opt => opt.MapFrom(src => src.SubCategories));
        CreateMap<CategoryRequestDto, Category>();

        // Transaction mappings
        CreateMap<Transaction, TransactionResponseDto>();
        CreateMap<TransactionRequestDto, Transaction>();

        // PaymentOrder mappings
        CreateMap<PaymentOrder, PaymentOrderResponseDto>();
        CreateMap<CreateTopUpRequestDto, PaymentOrder>();

        // Review mappings
        CreateMap<Review, ReviewResponseDto>()
            .ForMember(dest => dest.ReviewerFirstName,
                opt => opt.MapFrom(src => src.Reviewer.FirstName))
            .ForMember(dest => dest.ReviewerLastName,
                opt => opt.MapFrom(src => src.Reviewer.LastName));
        CreateMap<ReviewRequestDto, Review>();
    }
}