using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Services.AI;
using RecruitmentAPI.Services.Notification;

namespace RecruitmentAPI.Extensions
{
    /// <summary>
    /// Registers Kaveesha's module: AIService, NotificationService, AIScoreCalculator,
    /// and their external HTTP-backed dependencies. Call
    /// <see cref="AddKaveeshaModule"/> from Program.cs alongside the other
    /// members' AddXxxServices() calls.
    /// </summary>
    public static class KaveeshaServiceExtensions
    {
        /// <summary>
        /// Adds Kaveesha's AI service, notification service, and helper registrations
        /// to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">Application configuration for binding options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddKaveeshaModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Options binding
            services.Configure<AIServiceOptions>(configuration.GetSection(AIServiceOptions.SectionName));
            services.Configure<NotificationServiceOptions>(configuration.GetSection(NotificationServiceOptions.SectionName));

            // AI Service - typed HttpClient with a resilient timeout; retries can be layered on
            // with Microsoft.Extensions.Http.Resilience if desired.
            services.AddHttpClient<IAIService, AIService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Helpers
            services.AddScoped<AIScoreCalculator>();

            // Notification Service + provider-specific senders
            services.AddScoped<IEmailSender, SendGridEmailSender>();
            services.AddScoped<ISmsSender, TwilioSmsSender>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
