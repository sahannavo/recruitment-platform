using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RecruitmentAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RecruitmentAPI.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(User user, string role);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
        bool IsTokenExpired(string token);
        int? GetUserIdFromToken(string token);
    }

    /// <summary>
    /// Utility for generating and validating JSON Web Tokens.
    /// </summary>
    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtHelper> _logger;
        private readonly JwtSettings _jwtSettings;

        public JwtHelper(IConfiguration configuration, ILogger<JwtHelper> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _jwtSettings = LoadJwtSettings();
        }

        private JwtSettings LoadJwtSettings()
        {
            var settings = new JwtSettings();

            var jwtSection = _configuration.GetSection("Jwt");

            settings.Key = jwtSection["Key"] ??
                throw new InvalidOperationException("JWT:Key is not configured in appsettings");

            settings.Issuer = jwtSection["Issuer"] ??
                throw new InvalidOperationException("JWT:Issuer is not configured in appsettings");

            settings.Audience = jwtSection["Audience"] ??
                throw new InvalidOperationException("JWT:Audience is not configured in appsettings");

            if (!double.TryParse(jwtSection["ExpiryMinutes"] ?? "60", out var expiryMinutes))
            {
                _logger.LogWarning("Invalid JWT ExpiryMinutes, using default of 60 minutes");
                expiryMinutes = 60;
            }
            settings.ExpiryMinutes = expiryMinutes;

            if (settings.Key.Length < 32)
            {
                _logger.LogWarning("JWT key is less than 32 characters. This is a security risk.");
            }

            return settings;
        }

        public string GenerateToken(User user, string role)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        ClaimValueTypes.Integer64),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim("userId", user.UserId.ToString())
                };

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
                }

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                    signingCredentials: credentials);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogDebug("JWT generated successfully for user {UserId}", user.UserId);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT for user {UserId}", user.UserId);
                throw new InvalidOperationException("Failed to generate authentication token", ex);
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Attempted to validate null or empty token");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                {
                    _logger.LogWarning("Validated token is not a JWT token");
                    return null;
                }

                if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Token uses an insecure algorithm: {Algorithm}", jwtToken.Header.Alg);
                    return null;
                }

                _logger.LogDebug("Token validated successfully");
                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "Token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "Token has invalid signature");
                return null;
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                _logger.LogWarning(ex, "Token has invalid issuer");
                return null;
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                _logger.LogWarning(ex, "Token has invalid audience");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            _logger.LogDebug("Refresh token generated");
            return refreshToken;
        }

        public bool IsTokenExpired(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return null;
        }
    }

    /// <summary>
    /// JWT configuration settings.
    /// </summary>
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double ExpiryMinutes { get; set; } = 60;
    }
}