using System;
using System.Security.Claims;

namespace RecruitmentAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var claimValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? principal.FindFirst("id")?.Value;

            return int.TryParse(claimValue, out var id) ? id : throw new UnauthorizedAccessException("User identity claim not found.");
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}