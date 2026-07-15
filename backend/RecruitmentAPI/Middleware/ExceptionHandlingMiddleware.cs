using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Exceptions;

namespace RecruitmentAPI.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions and maps them to RFC 7807 ProblemDetails responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
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
            NotFoundException    => (StatusCodes.Status404NotFound,    "Resource Not Found"),
            BadRequestException  => (StatusCodes.Status400BadRequest,  "Bad Request"),
            UnauthorizedException=> (StatusCodes.Status401Unauthorized,"Unauthorized"),
            ForbiddenException   => (StatusCodes.Status403Forbidden,   "Forbidden"),
            AppException app     => (app.StatusCode,                   "Application Error"),
            _                    => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        // Only log unhandled exceptions (5xx) as errors; known app exceptions are warnings.
        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "Application exception ({Status}): {Message}",
                statusCode, exception.Message);

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, _jsonOptions));
    }
}

