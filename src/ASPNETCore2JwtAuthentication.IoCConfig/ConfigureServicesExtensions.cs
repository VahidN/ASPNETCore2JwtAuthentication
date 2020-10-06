using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASPNETCore2JwtAuthentication.Common;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.DomainClasses;
using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ASPNETCore2JwtAuthentication.IoCConfig
{
    public static class ConfigureServicesExtensions
    {
        public static void AddCustomAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(x => x.HeaderName = "X-XSRF-TOKEN");
            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
        }

        public static void AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .WithOrigins("http://localhost:4200") //Note:  The URL must be specified without a trailing slash (/).
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed((host) => true)
                        .AllowCredentials());
            });
        }

        public static void AddCustomJwtBearer(this IServiceCollection services, IConfiguration configuration)
        {
            // Only needed for custom roles.
            services.AddAuthorization(options =>
            {
                options.AddPolicy(CustomRoles.Admin, policy => policy.RequireRole(CustomRoles.Admin));
                options.AddPolicy(CustomRoles.User, policy => policy.RequireRole(CustomRoles.User));
                options.AddPolicy(CustomRoles.Editor, policy => policy.RequireRole(CustomRoles.Editor));
            });

            // Needed for jwt auth.
            services
                .AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["BearerTokens:Issuer"], // site that makes the token
                        ValidateIssuer = false, // TODO: change this to avoid forwarding attacks
                        ValidAudience = configuration["BearerTokens:Audience"], // site that consumes the token
                        ValidateAudience = false, // TODO: change this to avoid forwarding attacks
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["BearerTokens:Key"])),
                        ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                        ValidateLifetime = true, // validate the expiration
                        ClockSkew = TimeSpan.Zero // tolerance for the expiration date
                    };
                    cfg.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("Authentication failed.", context.Exception);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();
                            return tokenValidatorService.ValidateAsync(context);
                        },
                        OnMessageReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        public static void AddCustomDbContext(this IServiceCollection services, IConfiguration configuration, Assembly startupAssembly)
        {
            string projectDir = ServerPath.GetProjectPath(startupAssembly);
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("|DataDirectory|", Path.Combine(projectDir, "wwwroot", "app_data"));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString,
                        serverDbContextOptionsBuilder =>
                        {
                            var minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
                            serverDbContextOptionsBuilder.CommandTimeout(minutes);
                            serverDbContextOptionsBuilder.EnableRetryOnFailure();
                        });
            });
        }

        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAntiForgeryCookieService, AntiForgeryCookieService>();
            services.AddScoped<IUnitOfWork, ApplicationDbContext>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddSingleton<ISecurityService, SecurityService>();
            services.AddScoped<IDbInitializerService, DbInitializerService>();
            services.AddScoped<ITokenStoreService, TokenStoreService>();
            services.AddScoped<ITokenValidatorService, TokenValidatorService>();
            services.AddScoped<ITokenFactoryService, TokenFactoryService>();
        }

        public static void AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BearerTokensOptions>()
                                .Bind(configuration.GetSection("BearerTokens"))
                                .Validate(bearerTokens =>
                                {
                                    return bearerTokens.AccessTokenExpirationMinutes < bearerTokens.RefreshTokenExpirationMinutes;
                                }, "RefreshTokenExpirationMinutes is less than AccessTokenExpirationMinutes. Obtaining new tokens using the refresh token should happen only if the access token has expired.");
            services.AddOptions<ApiSettings>()
                    .Bind(configuration.GetSection("ApiSettings"));
        }
    }
}