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

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtHelper jwtHelper,
            IConfiguration configuration)
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

            // Create base user
            var newUser = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = request.Role ?? "Candidate",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            // Create role-specific profile
            var role = request.Role?.ToLower() ?? "candidate";
            switch (role)
            {
                case "candidate":
                    var candidate = new Candidate
                    {
                        UserId = newUser.UserId,
                        IsAvailable = true,
                        IsOpenToOpportunities = true
                    };
                    await _unitOfWork.Candidates.AddAsync(candidate);
                    break;

                case "recruiter":
                    var recruiter = new Recruiter
                    {
                        UserId = newUser.UserId,
                        Department = "General",
                        JobTitle = "Recruiter"
                    };
                    await _unitOfWork.Recruiters.AddAsync(recruiter);
                    break;

                case "hiringmanager":
                    var hiringManager = new HiringManager
                    {
                        UserId = newUser.UserId,
                        Department = "General"
                    };
                    await _unitOfWork.HiringManagers.AddAsync(hiringManager);
                    break;

                case "admin":
                case "superadmin":
                    var admin = new Admin
                    {
                        UserId = newUser.UserId,
                        Department = "General",
                        Permissions = "All"
                    };
                    await _unitOfWork.Admins.AddAsync(admin);
                    break;
            }

            await _unitOfWork.SaveChangesAsync();

            // Generate Token for immediate login after registration
            var token = _jwtHelper.GenerateToken(newUser, request.Role ?? "Candidate");
            var expiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiry,
                UserId = newUser.UserId,
                Email = newUser.Email,
                Role = newUser.Role,
                FirstName = newUser.FirstName,  // ✅ ADD
                LastName = newUser.LastName     // ✅ ADD
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null || !user.IsActive ||
                !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = _jwtHelper.GenerateToken(user, user.Role);
            var expiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiry,
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,  // ✅ ADD
                LastName = user.LastName     // ✅ ADD
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
            if (email == null)
                throw new UnauthorizedAccessException("Invalid token claims.");

            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive.");
            }

            var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                       ?? user.Role
                       ?? "Candidate";

            var newToken = _jwtHelper.GenerateToken(user, role);
            var expiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

            return new AuthResponse
            {
                Token = newToken,
                ExpiresAt = expiry,
                UserId = user.UserId,
                Email = user.Email,
                Role = role,
                FirstName = user.FirstName,  
                LastName = user.LastName     
            };
        }
    }
}