using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Repository.Implementations;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.AI;
using RecruitmentAPI.Services.Implementations;
using RecruitmentAPI.Services.Interfaces;
using RecruitmentAPI.Services.Notification;

namespace RecruitmentAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ─────────────────────────────────────────────────────────────────────────────
            // Repository layer
            // ─────────────────────────────────────────────────────────────────────────────
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IInterviewRepository, InterviewRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // ─────────────────────────────────────────────────────────────────────────────
            // Service layer
            // ─────────────────────────────────────────────────────────────────────────────
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IInterviewService, InterviewService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // ─────────────────────────────────────────────────────────────────────────────
            // Helper services
            // ─────────────────────────────────────────────────────────────────────────────
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtHelper, JwtHelper>();

            // ─────────────────────────────────────────────────────────────────────────────
            // Kaveesha's Module (AI + Notifications)
            // ─────────────────────────────────────────────────────────────────────────────
            services.Configure<AIServiceOptions>(configuration.GetSection(AIServiceOptions.SectionName));
            services.Configure<NotificationServiceOptions>(configuration.GetSection(NotificationServiceOptions.SectionName));

            services.AddHttpClient<IAIService, AIService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<AIScoreCalculator>();
            services.AddScoped<IEmailSender, SendGridEmailSender>();
            services.AddScoped<ISmsSender, TwilioSmsSender>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}