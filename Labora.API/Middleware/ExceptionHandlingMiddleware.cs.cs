using System.Net;
using System.Text.Json;
using Labora.Domain.Exceptions;

namespace Labora.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private const string GenericProductionMessage = "Kutilmagan xatolik yuz berdi.";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Kutilmagan xatolik: {Message}", exception.Message);
            await HandleExceptionAsync(httpContext, exception, _environment);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, IWebHostEnvironment environment)
    {
        httpContext.Response.ContentType = "application/json";

        int statusCode = exception switch
        {
            AlreadyReviewedException => (int)HttpStatusCode.Conflict,
            OtpConflictException => (int)HttpStatusCode.Conflict,
            OtpExpiredException => (int)HttpStatusCode.BadRequest,
            OtpInvalidOperationTokenException => (int)HttpStatusCode.BadRequest,
            OtpMaxAttemptsExceededException => (int)HttpStatusCode.BadRequest,
            OtpBlockedException => (int)HttpStatusCode.TooManyRequests,
            OtpSendRateLimitedException => (int)HttpStatusCode.TooManyRequests,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        // Every mapped case above already carries a curated, client-safe message. Only the unmapped
        // (500) fallback can surface arbitrary internal exception text, so only that case is
        // environment-gated - Development keeps the real message for local debugging, everything else
        // gets a fixed, generic message. The full exception, including this same message, is always
        // logged in InvokeAsync above regardless of environment.
        bool isUnmapped = statusCode == (int)HttpStatusCode.InternalServerError;
        string clientMessage = isUnmapped && !environment.IsDevelopment()
            ? GenericProductionMessage
            : exception.Message;

        object response = new
        {
            statusCode = statusCode,
            message = clientMessage
        };

        string jsonResponse = JsonSerializer.Serialize(response);
        await httpContext.Response.WriteAsync(jsonResponse);
    }
}