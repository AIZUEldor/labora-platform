using Labora.Application.Interfaces;
using Labora.Infrastructure.Options;
using Labora.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Labora.Application.Tests.Sms;

public class EskizValidateOnStartHostTests
{
    private static IHostBuilder CreateHostBuilder(IDictionary<string, string?> eskizValues)
    {
        Dictionary<string, string?> values = new(eskizValues)
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
        };

        // Bare HostBuilder (not Host.CreateDefaultBuilder) - no default console/event-log providers,
        // no external config sources. This is what actually exercises the generic Host's built-in
        // IStartupValidator hook: Host.StartAsync() resolves and runs it before any hosted service
        // starts, which is the real mechanism behind OptionsBuilder.ValidateOnStart().
        return new HostBuilder()
            .ConfigureAppConfiguration(configurationBuilder => configurationBuilder.AddInMemoryCollection(values))
            .ConfigureServices((context, services) => services.AddInfrastructure(context.Configuration));
    }

    [Fact]
    public async Task StartAsync_Disabled_NoCredentials_HostStartsSuccessfully()
    {
        using IHost host = CreateHostBuilder(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "false"
        }).Build();

        await host.StartAsync();
        await host.StopAsync();
    }

    [Fact]
    public async Task StartAsync_Enabled_MissingCredentials_FailsDuringHostStartup()
    {
        using IHost host = CreateHostBuilder(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "true"
        }).Build();

        // Asserting on host.StartAsync() itself - not on IOptions<EskizOptions>.Value - is the point:
        // it proves the generic Host's IStartupValidator fails startup eagerly, rather than the
        // failure being deferred until something manually touches .Value during a request.
        OptionsValidationException exception = await Assert.ThrowsAsync<OptionsValidationException>(
            () => host.StartAsync());

        Assert.Contains(exception.Failures, f => f.Contains("Eskiz:Email"));
        Assert.Contains(exception.Failures, f => f.Contains("Eskiz:Password"));
        Assert.Contains(exception.Failures, f => f.Contains("Eskiz:SenderName"));
    }

    [Fact]
    public async Task StartAsync_Enabled_ValidCredentials_HostStartsSuccessfully()
    {
        using IHost host = CreateHostBuilder(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "true",
            ["Eskiz:Email"] = "ops@labora.uz",
            ["Eskiz:Password"] = "secret",
            ["Eskiz:SenderName"] = "Labora",
        }).Build();

        await host.StartAsync();

        EskizOptions options = host.Services.GetRequiredService<IOptions<EskizOptions>>().Value;
        Assert.True(options.Enabled);
        Assert.IsType<EskizSmsSender>(host.Services.GetRequiredService<ISmsSender>());

        await host.StopAsync();
    }
}
