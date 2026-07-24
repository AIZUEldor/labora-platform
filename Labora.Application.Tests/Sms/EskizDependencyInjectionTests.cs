using Labora.Application.Interfaces;
using Labora.Infrastructure.Options;
using Labora.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Labora.Application.Tests.Sms;

public class EskizDependencyInjectionTests
{
    private static IConfiguration BuildConfiguration(IDictionary<string, string?> eskizValues)
    {
        Dictionary<string, string?> values = new(eskizValues)
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
        };

        return new ConfigurationBuilder().AddInMemoryCollection(values!).Build();
    }

    [Fact]
    public void AddInfrastructure_Disabled_NoCredentials_OptionsResolveWithoutThrowing()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "false"
        });

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        // Must not throw OptionsValidationException even though Email/Password/SenderName are
        // entirely absent from configuration - this is the exact production scenario (Eskiz
        // contract not signed yet).
        EskizOptions options = provider.GetRequiredService<IOptions<EskizOptions>>().Value;

        Assert.False(options.Enabled);
    }

    [Fact]
    public void AddInfrastructure_Disabled_ResolvesDisabledSmsSender()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "false"
        });

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        ISmsSender sender = provider.GetRequiredService<ISmsSender>();

        Assert.IsType<DisabledSmsSender>(sender);
    }

    [Fact]
    public void AddInfrastructure_Disabled_EskizNamedHttpClientIsNeverConfigured()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "false"
        });

        ServiceCollection services = new();
        // Registers the base IHttpClientFactory infrastructure independently of AddInfrastructure -
        // in the real app this always exists (Program.cs also registers an "ExpoPush" client), so
        // this mirrors that rather than testing an artifact of this test's isolation.
        services.AddHttpClient();
        services.AddInfrastructure(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        // AddHttpClient(EskizSmsSender.HttpClientName, ...) was never called while disabled, so the
        // factory falls back to an unconfigured default client - no BaseAddress was ever set,
        // proving the Eskiz-specific configuration delegate never ran (and therefore never had a
        // chance to make an outbound call).
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
        HttpClient client = factory.CreateClient(EskizSmsSender.HttpClientName);

        Assert.Null(client.BaseAddress);
    }

    [Fact]
    public void AddInfrastructure_Enabled_MissingCredentials_ThrowsOptionsValidationException()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "true"
        });

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<EskizOptions>>().Value);
    }

    [Theory]
    [InlineData("Eskiz:Email")]
    [InlineData("Eskiz:Password")]
    [InlineData("Eskiz:SenderName")]
    public void AddInfrastructure_Enabled_MissingSingleField_ThrowsOptionsValidationException(string missingKey)
    {
        Dictionary<string, string?> values = new()
        {
            ["Eskiz:Enabled"] = "true",
            ["Eskiz:Email"] = "ops@labora.uz",
            ["Eskiz:Password"] = "secret",
            ["Eskiz:SenderName"] = "Labora",
        };
        values.Remove(missingKey);

        IConfiguration configuration = BuildConfiguration(values);

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<EskizOptions>>().Value);
    }

    [Fact]
    public void AddInfrastructure_Enabled_ValidCredentials_ResolvesEskizSmsSender()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "true",
            ["Eskiz:Email"] = "ops@labora.uz",
            ["Eskiz:Password"] = "secret",
            ["Eskiz:SenderName"] = "Labora",
        });

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();

        EskizOptions options = provider.GetRequiredService<IOptions<EskizOptions>>().Value;
        ISmsSender sender = provider.GetRequiredService<ISmsSender>();

        Assert.True(options.Enabled);
        Assert.IsType<EskizSmsSender>(sender);
    }
}
