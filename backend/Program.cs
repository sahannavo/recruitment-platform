using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecruitmentAPI.Extensions;
using RecruitmentAPI.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICE REGISTRATION (Dependency Injection)
// ==========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🔌 Use your custom Extension Methods
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddServices();

// 🌐 Configure CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500") // Adjust to match your frontend ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 🔐 Configure Standard JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration."));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Strictly adhere to expiration times
        };
    });

builder.Services.AddAuthorization();

// 📖 Configure Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Recruitment API",
        Version = "v1",
        Description = "AI-Powered Recruitment Platform - Backend API"
    });

    // Define the Bearer Authorization scheme for the Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1Ni...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Build the application
var app = builder.Build();


// ==========================================
// 2. HTTP REQUEST PIPELINE (Middleware Order)
// ==========================================

// 1. Global Exception Handler (Catches errors from everything below it)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 📖 2. Swagger UI (Available in Development mode)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🌐 3. CORS (Must be applied before Auth and Routing)
app.UseCors("AllowFrontend");

// 🔐 4. Standard Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 🧑‍💻 5. Custom JWT Middleware (Extracts User and attaches to HttpContext)
app.UseMiddleware<JwtMiddleware>();

// 📝 6. Audit Logging Middleware (Logs the action, utilizing the User injected above)
app.UseMiddleware<AuditLoggingMiddleware>();

// 🎯 7. Map Controller Endpoints
app.MapControllers();


// ==========================================
// 3. STARTUP EXECUTION
// ==========================================

// 💾 Automatically apply any pending Entity Framework database migrations
app.ApplyDatabaseMigrations();

// Start the app
app.Run();