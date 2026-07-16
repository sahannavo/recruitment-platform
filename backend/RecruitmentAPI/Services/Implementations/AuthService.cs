using RecruitmentAPI.DTOs.Auth;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtHelper jwtHelper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Map DTO to appropriate Entity based on requested role
            User newUser = request.Role.ToLower() switch
            {
                "recruiter" => new Recruiter { Department = "General" },
                "hiringmanager" => new HiringManager { Department = "General" },
                _ => new Candidate() // Default to Candidate
            };

            newUser.Email = request.Email;
            newUser.FirstName = request.FirstName;
            newUser.LastName = request.LastName;
            newUser.PasswordHash = _passwordHasher.HashPassword(request.Password);
            newUser.Role = request.Role; // Set Role on the base User object
            newUser.CreatedAt = DateTime.UtcNow;
            newUser.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            // If the role is Admin, create Admin profile after User is saved
            if (request.Role.ToLower() == "admin")
            {
                var adminProfile = new Admin
                {
                    UserId = newUser.UserId,
                    Department = "General",
                    Permissions = ""
                };
                await _unitOfWork.Admins.AddAsync(adminProfile);
                await _unitOfWork.SaveChangesAsync();
            }

            // Generate Token for immediate login after registration
            var token = _jwtHelper.GenerateToken(newUser, request.Role);
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!));

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiry,
                UserId = newUser.UserId,
                Email = newUser.Email,
                Role = request.Role
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null || !user.IsActive || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Determine Role based on user.Role property
            string role = user.Role;

            var token = _jwtHelper.GenerateToken(user, role);
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!));

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiry,
                UserId = user.UserId,
                Email = user.Email,
                Role = role
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token)
        {
            var principal = _jwtHelper.ValidateToken(token);
            if (principal == null)
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }

            var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (email == null) throw new UnauthorizedAccessException("Invalid token claims.");

            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive.");
            }

            var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Candidate";

            var newToken = _jwtHelper.GenerateToken(user, role);
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!));

            return new AuthResponse
            {
                Token = newToken,
                ExpiresAt = expiry,
                UserId = user.UserId,
                Email = user.Email,
                Role = role
            };
        }
    }
}