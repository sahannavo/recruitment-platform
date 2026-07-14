using RecruitmentAPI.Helpers;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtHelper jwtHelper, IUnitOfWork unitOfWork)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await AttachUserToContext(context, jwtHelper, unitOfWork, token);
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, IJwtHelper jwtHelper, IUnitOfWork unitOfWork, string token)
        {
            try
            {
                var principal = jwtHelper.ValidateToken(token);
                if (principal != null)
                {
                    // Extract the User ID from the JWT claims
                    var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        // Attach user to context on successful JWT validation
                        context.Items["User"] = await unitOfWork.Users.GetByIdAsync(userId);
                    }
                }
            }
            catch
            {
                // Do nothing if JWT validation fails
                // User is not attached to context, so the request won't have access to secure routes
            }
        }
    }
}