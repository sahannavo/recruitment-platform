using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.DTOs.Auth;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Controllers
{
    /// <summary>
    /// Handles user authentication, registration, and token management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                _logger.LogInformation("User {Email} logged in successfully.", request.Email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt for {Email}.", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                _logger.LogInformation("New user registered: {Email} with role {Role}.", request.Email, request.Role);
                return CreatedAtAction(nameof(Login), new { email = response.Email }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "No token provided." });
                }

                var response = await _authService.RefreshTokenAsync(token);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // JWT is stateless, so we handle logout on the client side by destroying the token.
            // If utilizing refresh tokens stored in a database or cookies, you would invalidate them here.
            _logger.LogInformation("User logged out. Token must be removed on client side.");
            return Ok(new { message = "Logged out successfully. Please remove token from client." });
        }
    }
}