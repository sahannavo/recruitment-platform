using System.Net;
using System.Text.Json;
using RecruitmentAPI.Exceptions;

namespace RecruitmentAPI.Middleware;

/// <summary>
/// Global exception handling middleware that converts exceptions to RFC 7807 ProblemDetails responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            AppException appEx => (appEx.StatusCode, GetTitle(appEx.StatusCode)),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Application exception: {Message}", exception.Message);
        }

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message,
            instance = context.Request.Path.Value
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        _ => "Internal Server Error"
    };
}

