using System.Text.Json;
using Labora.API.Middleware;
using Labora.Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace Labora.Application.Tests.Sms;

public class ExceptionHandlingMiddlewareSmsTests
{
    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "Labora.API.Tests";
        public string ContentRootPath { get; set; } = ".";
        public string WebRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static async Task<(int StatusCode, string Body)> InvokeMiddlewareAsync(Exception exceptionToThrow, string environmentName = "Production")
    {
        ExceptionHandlingMiddleware middleware = new(
            _ => throw exceptionToThrow,
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            new FakeWebHostEnvironment { EnvironmentName = environmentName });

        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(context.Response.Body);
        string body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task InvokeAsync_SmsProviderUnavailableException_Maps503()
    {
        (int statusCode, string body) = await InvokeMiddlewareAsync(
            new SmsProviderUnavailableException("SMS sending is currently disabled."));

        Assert.Equal(503, statusCode);

        using JsonDocument document = JsonDocument.Parse(body);
        Assert.Equal(503, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("SMS sending is currently disabled.", document.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_SmsProviderUnavailableException_UsesExistingResponseShape()
    {
        (_, string body) = await InvokeMiddlewareAsync(
            new SmsProviderUnavailableException("SMS sending is currently disabled."));

        using JsonDocument document = JsonDocument.Parse(body);
        List<string> propertyNames = document.RootElement.EnumerateObject().Select(p => p.Name).ToList();

        Assert.Equal(new[] { "statusCode", "message" }, propertyNames);
    }

    [Fact]
    public async Task InvokeAsync_SmsProviderUnavailableException_DoesNotExposeConfigurationDetails()
    {
        (_, string body) = await InvokeMiddlewareAsync(
            new SmsProviderUnavailableException("SMS sending is currently disabled."));

        Assert.DoesNotContain("Email", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("eskiz.uz", body, StringComparison.OrdinalIgnoreCase);
    }
}
