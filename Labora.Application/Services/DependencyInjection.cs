using Labora.Application.Interfaces;
using Labora.Application.Mappings;
using Labora.Application.Options;
using Labora.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Labora.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddOptions<OtpOptions>()
            .Bind(configuration.GetSection(OtpOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<IdentifierHashOptions>()
            .Bind(configuration.GetSection(IdentifierHashOptions.SectionName))
            .Validate(options =>
            {
                if (string.IsNullOrWhiteSpace(options.Pepper))
                {
                    return false;
                }

                try
                {
                    return Convert.FromBase64String(options.Pepper).Length == 32;
                }
                catch (FormatException)
                {
                    return false;
                }
            }, "OtpIdentity:Pepper must be a base64-encoded 32-byte (256-bit) value, distinct from Otp:Pepper.")
            .ValidateOnStart();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IOtpSecurityService, OtpSecurityService>();
        services.AddScoped<IIdentifierHasher, IdentifierHasher>();
        services.AddScoped<IOtpRequestContextProvider, OtpRequestContextProvider>();
        services.AddScoped<IOtpAbuseGuard, OtpAbuseGuard>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IPhoneNumberNormalizer, UzbekistanPhoneNumberNormalizer>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}