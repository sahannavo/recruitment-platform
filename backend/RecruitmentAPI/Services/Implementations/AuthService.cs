using RecruitmentAPI.DTOs.Auth;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace RecruitmentAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IRecruiterRepository _recruiterRepository;
        private readonly IHiringManagerRepository _hiringManagerRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            IRecruiterRepository recruiterRepository,
            IHiringManagerRepository hiringManagerRepository,
            IAdminRepository adminRepository,
            IPasswordHasher passwordHasher,
            IJwtHelper jwtHelper,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _recruiterRepository = recruiterRepository;
            _hiringManagerRepository = hiringManagerRepository;
            _adminRepository = adminRepository;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
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

            await _userRepository.AddAsync(newUser);
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
                    await _candidateRepository.AddAsync(candidate);
                    break;

                case "recruiter":
                    var recruiter = new Recruiter
                    {
                        UserId = newUser.UserId,
                        Department = "General",
                        JobTitle = "Recruiter"
                    };
                    await _recruiterRepository.AddAsync(recruiter);
                    break;

                    // ... etc for other roles
            }

            await _unitOfWork.SaveChangesAsync();

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
                FirstName = newUser.FirstName,
                LastName = newUser.LastName
            };
        }

        // LoginAsync and RefreshTokenAsync...
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

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
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token)
        {
            var principal = _jwtHelper.ValidateToken(token);
            if (principal == null)
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }

            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value
                             ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
                throw new UnauthorizedAccessException("Invalid token claims.");

            var user = await _userRepository.GetByEmailAsync(emailClaim);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive.");
            }

            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? user.Role;
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