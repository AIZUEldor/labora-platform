using Labora.Application.Interfaces;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Labora.Infrastructure.Options;
using Labora.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Labora.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<LaboaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IOtpAbuseEventRepository, OtpAbuseEventRepository>();
        services.AddScoped<IOtpBlockRepository, OtpBlockRepository>();
        services.AddScoped<IPaymeTransactionRepository, PaymeTransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileStorageService, LocalDiskFileStorageService>();

        // Email/Password/SenderName are only required when Enabled is true - the Eskiz contract is
        // not signed yet, so production currently runs with Enabled=false and no credentials at all.
        // These replace the [Required] attributes EskizOptions used to carry, since attribute-based
        // validation can't be made conditional on a sibling property.
        services.AddOptions<EskizOptions>()
            .Bind(configuration.GetSection(EskizOptions.SectionName))
            .Validate(o => !o.Enabled || !string.IsNullOrWhiteSpace(o.Email),
                "Eskiz:Email is required when Eskiz:Enabled is true.")
            .Validate(o => !o.Enabled || !string.IsNullOrWhiteSpace(o.Password),
                "Eskiz:Password is required when Eskiz:Enabled is true.")
            .Validate(o => !o.Enabled || !string.IsNullOrWhiteSpace(o.SenderName),
                "Eskiz:SenderName is required when Eskiz:Enabled is true.")
            .ValidateOnStart();

        // Read the raw config value directly (not through the IOptions pipeline above) so the
        // decision of which ISmsSender to register happens at DI-configuration time. When disabled,
        // EskizSmsSender and its named HttpClient are never registered at all - not just unused -
        // which is what guarantees zero possibility of an outbound Eskiz HTTP call while disabled.
        bool eskizEnabled = configuration.GetValue<bool>($"{EskizOptions.SectionName}:{nameof(EskizOptions.Enabled)}");

        if (eskizEnabled)
        {
            services.AddHttpClient(EskizSmsSender.HttpClientName, (serviceProvider, client) =>
            {
                EskizOptions options = serviceProvider.GetRequiredService<IOptions<EskizOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            // Singleton, not Scoped/Transient: EskizSmsSender caches its login token across requests
            // in process memory (SemaphoreSlim-guarded single-flight refresh) - a shorter lifetime
            // would silently reset that cache on every resolution and force a fresh login per SMS.
            // The HttpClient it uses still comes from IHttpClientFactory on every call via the named
            // registration above, so connection pooling/DNS refresh is unaffected by this lifetime.
            services.AddSingleton<ISmsSender, EskizSmsSender>();
        }
        else
        {
            services.AddSingleton<ISmsSender, DisabledSmsSender>();
        }

        return services;
    }
}