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
            .Validate(options => IsValidPepper(options.Pepper),
                "OtpIdentity:Pepper must be a base64-encoded 32-byte (256-bit) value, distinct from Otp:Pepper.")
            .ValidateOnStart();

        services.AddOptions<OtpSecurityOptions>()
            .Bind(configuration.GetSection(OtpSecurityOptions.SectionName))
            .Validate(options => IsValidPepper(options.Pepper),
                "Otp:Pepper must be a base64-encoded 32-byte (256-bit) value.")
            .ValidateOnStart();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IOtpSecurityService, OtpSecurityService>();
        services.AddScoped<IIdentifierHasher, IdentifierHasher>();
        services.AddScoped<IOtpRequestContextProvider, OtpRequestContextProvider>();
        services.AddScoped<IOtpAbuseGuard, OtpAbuseGuard>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
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

        // Payme
        services.AddScoped<IPaymeCheckoutUrlBuilder, PaymeCheckoutUrlBuilder>();
        services.AddScoped<IPaymeAuthenticator, PaymeAuthenticator>();
        services.AddScoped<IPaymeMerchantService, PaymeMerchantService>();

        // Intentionally no .ValidateOnStart() here (unlike the Otp/OtpIdentity/Eskiz options above):
        // no Payme:Login/Payme:Password values exist in any environment's configuration yet, and no
        // controller resolves IPaymeMerchantService/IPaymeAuthenticator today, so nothing forces this
        // to validate. ValidateOnStart() would fail application startup in every environment as soon
        // as this line ran. Add it back once real Payme credentials are configured and a controller
        // is wired up to actually use them.
        services.AddOptions<PaymeMerchantOptions>()
            .Bind(configuration.GetSection(PaymeMerchantOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }

    /// <summary>
    /// Shared by the OtpIdentity and Otp pepper ValidateOnStart() checks above - both peppers must be
    /// a base64-encoded 32-byte (256-bit) value, so the predicate lives once here instead of being
    /// duplicated per options class.
    /// </summary>
    private static bool IsValidPepper(string? pepper)
    {
        if (string.IsNullOrWhiteSpace(pepper))
        {
            return false;
        }

        try
        {
            return Convert.FromBase64String(pepper).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}