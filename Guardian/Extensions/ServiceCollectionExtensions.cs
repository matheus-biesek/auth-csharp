using FluentValidation;
using Guardian.Data;
using Guardian.Mappings;
using Guardian.Services.Common;
using Guardian.Services.V1;
using Guardian.Services.V2;
using Guardian.Services.v1;
using Guardian.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace Guardian.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGuardianServices(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRedisService, RedisService>();
        services.AddScoped<IHealthServiceV1, HealthServiceV1>();
        services.AddScoped<IHealthServiceV2, HealthServiceV2>();
        services.AddScoped<IAuthServiceV1, AuthServiceV1>();

        return services;
    }

    public static IServiceCollection AddGuardianApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddGuardianDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    public static IServiceCollection AddGuardianIdentity(this IServiceCollection services)
    {
        // Registra Identity com User e Role customizados
        // Agora usando AddIdentity (não apenas AddIdentityCore) para ter suporte a roles
        services.AddIdentity<Guardian.Models.User, Guardian.Models.Role>(options =>
        {
            // Configuração de senha
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // Configuração de usuário
            options.User.RequireUniqueEmail = true; // Email único
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Configuração de lockout
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders(); // Necessário para reset de senha, etc.

        return services;
    }

    public static IServiceCollection AddGuardianRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnection ?? "localhost:6379")
        );

        return services;
    }

    public static IServiceCollection AddGuardianJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found");
        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "Guardian.API",
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"] ?? "Guardian.Client",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Configurar para ler token de cookies quando não estiver no header Authorization
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Se não há token no header Authorization, tenta ler do cookie
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        if (context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddGuardianAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AuthMapperProfile));
        return services;
    }

    public static IServiceCollection AddGuardianFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }
}

