using System.Text;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.BackgroundServices;
using AutoStack.Infrastructure.Options;
using AutoStack.Infrastructure.Persistence;
using AutoStack.Infrastructure.Repositories;
using AutoStack.Infrastructure.Security;
using AutoStack.Infrastructure.Security.Handlers;
using AutoStack.Infrastructure.Security.Models;
using AutoStack.Infrastructure.Security.Requirements;
using AutoStack.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Resend;

namespace AutoStack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<Security.Models.CookieSettings>(configuration.GetSection("CookieSettings"));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<TwoFactorSettings>(configuration.GetSection("TwoFactorSettings"));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                // Enable connection pooling with reasonable limits
                npgsqlOptions.MinBatchSize(1);
                npgsqlOptions.MaxBatchSize(100);

                // Set command timeout to 30 seconds (default is 30, but making it explicit)
                npgsqlOptions.CommandTimeout(30);

                // Enable retry on failure for transient errors
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // Enable sensitive data logging only in development
            if (configuration.GetValue<string>("environment") == "development")
            {
                options.EnableSensitiveDataLogging();
            }

            // Log slow queries (over 1 second)
            options.LogTo((eventId, level) => level >= LogLevel.Warning ||
                         eventId.Name == "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted",
                (eventData) =>
                {
                    switch (eventData)
                    {
                        case Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData commandData:
                        {
                            if (commandData.Duration.TotalMilliseconds > 1000)
                            {
                                Console.WriteLine($"[SLOW QUERY] {commandData.Duration.TotalMilliseconds}ms: {commandData.Command.CommandText}");
                            }

                            break;
                        }
                        
                        default:
                            Console.WriteLine($"[ERROR] {eventData}");
                            break;
                    }
                });
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IStackRepository, StackRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRecoveryCodeRepository, RecoveryCodeRepository>();
        services.AddScoped<IStackVoteRepository, StackVoteRepository>();
        services.AddScoped<ICliVersionRepository, CliVersionRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthentication, Authentication>();
        services.AddScoped<IToken, Token>();
        services.AddScoped<ICookieManager, CookieManager>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IRecoveryCodeGenerator, RecoveryCodeGenerator>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddHostedService<LogCleanupBackgroundService>();

        // Email service (Resend)
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration["ResendClient:ApiToken"];
        });
        services.AddTransient<IResend, ResendClient>();

        return services;
    }

    public static IServiceCollection AddAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (jwtSettings == null)
            throw new InvalidOperationException("JwtSettings configuration is missing");

        services.AddAuthorization(options =>
            {
                options.AddPolicy("EmailVerified", policy =>
                    policy.Requirements.Add(new EmailVerifiedRequirement()));
            })
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddScoped<IAuthorizationHandler, EmailVerifiedHandler>();

        return services;
    }
}