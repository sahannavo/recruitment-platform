using Microsoft.Extensions.DependencyInjection;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Repository.Implementations;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Implementations;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Extensions
{
    /// <summary>
    /// Extension methods to keep Program.cs clean by grouping service registrations.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register Generic Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register Specific Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // 💡 Team members (Savindi, Sobani, Sandawaruni) will add their repositories here:
            // services.AddScoped<IJobRepository, JobRepository>();
            // services.AddScoped<IApplicationRepository, ApplicationRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Register Helpers (often Singletons or Scoped depending on state)
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtHelper, JwtHelper>();
            services.AddScoped<IAIScoreCalculator, AIScoreCalculator>(); // Kaveesha will update this later

            // Register Business Services
            services.AddScoped<IAuthService, AuthService>();

            // 💡 Team members will add their services here:
            // services.AddScoped<ICandidateService, CandidateService>();
            // services.AddScoped<IInterviewService, InterviewService>();

            return services;
        }
    }
}