using RecruitmentAPI.DTOs.Auth;

namespace RecruitmentAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token);
        Task<bool> LogoutAsync(int userId);
        Task<bool> ValidateTokenAsync(string token);
    }
}