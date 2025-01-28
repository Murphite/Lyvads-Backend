using Lyvads.Application.Interfaces;
using Lyvads.Application.Implementations;
using Lyvads.Domain.Interfaces;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;
using System.Text;
using Lyvads.Application.Utilities;

namespace Lyvads.API.Extensions;

public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Fast Api",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
        {
            //var key = Encoding.UTF8.GetBytes(configuration.GetSection("JWT:Key").Value);
            var jwtKey = configuration.GetSection("JWT:Key").Value;

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException(nameof(jwtKey), "JWT key is missing in the configuration.");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
            };
        });

        services.AddCors(options =>
        {
            // General-purpose policy for Paystack webhook
            options.AddPolicy("AllowPaystack",
            builder =>
            {
                builder.WithOrigins("https://api.paystack.co") // Replace or add Paystack-specific domains as needed
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });

        services.AddDistributedMemoryCache(); 
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true; 
        }); 

        // Register Application Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICreatorService, CreatorService>();
        services.AddScoped<IUserInteractionService, UserInteractionService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWaitlistService, WaitlistService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRegularUserService, RegularUserService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAdminChargeTransactionService, AdminChargeTransactionService>();
        services.AddScoped<IAdminUserService, AdminDashboardService>();
        services.AddScoped<ISuperAdminService, AdminUserService>();
        services.AddScoped<IAdminPostService, AdminPostService>();
        services.AddScoped<IAdminChargeTransactionService, AdminChargeTransactionService>();
        services.AddScoped<ICollaborationService, CollaborationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDisputeService, AdminDisputeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPromotionService, AdminPromotionService>();
        services.AddScoped<IUserAdService, AdminUserAdService>();
        services.AddScoped<IEmailContext, EmailContext>();
        services.AddScoped<IAdminActivityLogService, AdminActivityLogService>();
        services.AddScoped<IAdminChargeTransactionService, AdminChargeTransactionService>();
        services.AddScoped<IAdminPermissionsService, AdminPermissionsService>();

        // Register Repositories
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IRepository, Repository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRegularUserRepository, RegularUserRepository>();
        services.AddScoped<ICreatorRepository, CreatorRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
        services.AddScoped<IDisputeRepository, DisputeRepository >();
        services.AddScoped<IImpressionRepository, ImpressionRepository >();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserAdRepository, UserAdRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<ICollaborationRepository, CollaborationRepository>();
        services.AddScoped<IChargeTransactionRepository, ChargeTransactionRepository>();
        services.AddScoped<IAdminPermissionsRepository, AdminPermissionsRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        //other services
        services.AddHttpContextAccessor();
        services.AddScoped<WebSocketHandler>();
        services.AddHttpContextAccessor();
        services.AddHttpClient();

    }
}
