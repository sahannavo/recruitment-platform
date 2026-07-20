using System.Security.Claims;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public JwtMiddleware(
            RequestDelegate next,
            ILogger<JwtMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context, IJwtHelper jwtHelper)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                await AttachUserToContext(context, jwtHelper, token);
            }

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, IJwtHelper jwtHelper, string token)
        {
            try
            {
                var principal = jwtHelper.ValidateToken(token);
                if (principal != null)
                {
                    var userIdClaim = principal.FindFirst("sub")?.Value
                                   ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                    {
                        // Create a scope to resolve scoped services
                        using var scope = _scopeFactory.CreateScope();
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                        var user = await userRepository.GetByIdAsync(userId);
                        if (user != null)
                        {
                            context.Items["User"] = user;
                            _logger.LogDebug("User {UserId} attached to context", userId);
                        }
                        else
                        {
                            _logger.LogWarning("User {UserId} not found in database", userId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid user ID claim in token");
                    }
                }
                else
                {
                    _logger.LogDebug("Token validation failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching user to context");
            }
        }
    }
}