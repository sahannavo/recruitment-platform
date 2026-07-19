using System.Security.Claims;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Models;

namespace RecruitmentAPI.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;
        private readonly IUserRepository _userRepository;

        public JwtMiddleware(
            RequestDelegate next,
            ILogger<JwtMiddleware> logger,
            IUserRepository userRepository)
        {
            _next = next;
            _logger = logger;
            _userRepository = userRepository;
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
                        var user = await _userRepository.GetByIdAsync(userId);
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching user to context");
            }
        }
    }
}