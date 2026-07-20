using System.Net;
using System.Text.Json;
using Labora.Domain.Exceptions;
using Labora.Domain.Exceptions;

namespace Labora.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
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

        object response = new
        {
            statusCode = statusCode,
            message = exception.Message
        };

        string jsonResponse = JsonSerializer.Serialize(response);
        await httpContext.Response.WriteAsync(jsonResponse);
    }
}