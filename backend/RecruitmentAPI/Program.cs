using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecruitmentAPI.Data;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Middleware;
using RecruitmentAPI.Repository.Implementations;
using RecruitmentAPI.Repository.Interfaces;
using RecruitmentAPI.Services.Implementations;
using RecruitmentAPI.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RecruitmentAPI
{
    // Swagger schema filter to handle IFormFile parameters (file uploads)
    internal class FormFileSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Microsoft.AspNetCore.Http.IFormFile))
            {
                schema.Type = "string";
                schema.Format = "binary";
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ────────────────────────────────────────────────────────────────────────────-
            // MVC + API Explorer
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ─────────────────────────────────────────────────────────────────────────────
            // CORS Configuration
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins(
                                "http://localhost:5500",      // VS Code Live Server
                                "https://localhost:5500",
                                "http://127.0.0.1:5500",
                                "http://localhost:3000",      // React
                                "https://localhost:3000",
                                "http://localhost:8080",      // Vue.js
                                "https://localhost:8080",
                                "http://localhost:4200"       // Angular
                            )
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials(); // Required for cookies/auth
                    });
            });

            // ─────────────────────────────────────────────────────────────────────────────
            // Swagger / OpenAPI
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Recruitment Platform API",
                    Version = "v1",
                    Description = "AI-Powered Recruitment and Talent Management Platform API"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.SchemaFilter<FormFileSchemaFilter>();

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            // ─────────────────────────────────────────────────────────────────────────────
            // Database
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ─────────────────────────────────────────────────────────────────────────────
            // Repository layer
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
            builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // ─────────────────────────────────────────────────────────────────────────────
            // Service layer - ALL services registered
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICandidateService, CandidateService>();
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddScoped<IInterviewService, InterviewService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAIService, AIService>();
            builder.Services.Configure<AIServiceOptions>(builder.Configuration.GetSection(AIServiceOptions.SectionName));

            // ─────────────────────────────────────────────────────────────────────────────
            // AI Service with HttpClient
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.Configure<AIServiceOptions>(
                builder.Configuration.GetSection(AIServiceOptions.SectionName));

            builder.Services.AddHttpClient<IAIService, AIService>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AIServiceOptions>>().Value;
    
                if (!string.IsNullOrEmpty(options.BaseUrl))
                {
                    client.BaseAddress = new Uri(options.BaseUrl);
                }
                else if (!string.IsNullOrEmpty(options.Endpoint))
                {
                    client.BaseAddress = new Uri(options.Endpoint);
                }
    
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    
                // Set default headers for OpenRouter
                if (options.Provider?.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase) == true)
                {
                    client.DefaultRequestHeaders.Add("HTTP-Referer", "https://recruitai.com");
                    client.DefaultRequestHeaders.Add("X-Title", "RecruitAI Platform");
                }
            });

            // Also register as scoped for dependency injection
            builder.Services.AddScoped<IAIService, AIService>();
            builder.Services.AddScoped<AIScoreCalculator>();

            // ─────────────────────────────────────────────────────────────────────────────
            // Helper services
            // ─────────────────────────────────────────────────────────────────────────────
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtHelper, JwtHelper>();

            // ─────────────────────────────────────────────────────────────────────────────
            // JWT Authentication
            // ─────────────────────────────────────────────────────────────────────────────
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured.");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                                                       Encoding.UTF8.GetBytes(jwtKey)),
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role
                    };
                });

            builder.Services.AddAuthorization();

            // ─────────────────────────────────────────────────────────────────────────────
            // Build pipeline
            // ─────────────────────────────────────────────────────────────────────────────
            var app = builder.Build();

            // Global exception handling - MUST BE FIRST
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // JWT Middleware for custom token validation
            app.UseMiddleware<JwtMiddleware>();

            // Use CORS
            app.UseCors("AllowFrontend");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recruitment Platform API v1"));
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}