using RecruitmentAPI.Repository.Implementations;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Implementations;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Extensions;

/// <summary>
/// Extension methods for registering Interview and Feedback module services
/// </summary>
public static class InterviewModuleExtensions
{
    /// <summary>
    /// Add Interview and Feedback repositories and services to the DI container
    /// </summary>
    public static IServiceCollection AddInterviewModule(this IServiceCollection services)
    {
        // Register Repositories
        services.AddScoped<IInterviewRepository, InterviewRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();

        // Register Services
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IFeedbackService, FeedbackService>();

        return services;
    }
}
