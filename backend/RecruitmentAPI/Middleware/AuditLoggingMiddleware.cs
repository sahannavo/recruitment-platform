using RecruitmentAPI.Models;
using System.Diagnostics;

namespace RecruitmentAPI.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Let the request proceed
            await _next(context);

            stopwatch.Stop();

            // After the request is handled, log the details
            LogAuditTrail(context, stopwatch.ElapsedMilliseconds);
        }

        private void LogAuditTrail(HttpContext context, long executionTimeMs)
        {
            var method = context.Request.Method;
            var path = context.Request.Path;
            var statusCode = context.Response.StatusCode;

            // Check if the user was attached by the JwtMiddleware
            string userIdentifier = "Anonymous";
            if (context.Items["User"] is User user)
            {
                userIdentifier = $"User:{user.UserId} ({user.Email})";
            }

            _logger.LogInformation(
                "Audit Log: [{Method}] {Path} - Status: {StatusCode} - Executed by: {UserIdentifier} in {ExecutionTime}ms",
                method, path, statusCode, userIdentifier, executionTimeMs);
        }
    }
}