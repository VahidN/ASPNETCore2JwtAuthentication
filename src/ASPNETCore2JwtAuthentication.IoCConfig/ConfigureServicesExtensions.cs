using System.Text;
using ASPNETCore2JwtAuthentication.Common;
using ASPNETCore2JwtAuthentication.DataLayer.Context;
using ASPNETCore2JwtAuthentication.Models;
using ASPNETCore2JwtAuthentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ASPNETCore2JwtAuthentication.IoCConfig;

public static class ConfigureServicesExtensions
{
    public static void AddCustomAntiforgery(this IServiceCollection services)
    {
        services.AddAntiforgery(x => x.HeaderName = "X-XSRF-TOKEN");
        services.AddMvc(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); });
    }

    public static void AddCustomCors(this IServiceCollection services)
        => services.AddCors(options =>
        {
            options.AddPolicy(name: "CorsPolicy", builder
                => builder
                    .WithOrigins(
                        "http://localhost:4200") //Note:  The URL must be specified without a trailing slash (/).
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(host => true)
                    .AllowCredentials());
        });

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
        services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                var bearerTokenOption = configuration.GetSection(key: "BearerTokens").Get<BearerTokensOptions>() ?? throw new InvalidOperationException(message: "bearerTokenOption is null");
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    // site that makes the token
                    ValidIssuer = bearerTokenOption.Issuer,
                    ValidateIssuer = true,

                    // site that consumes the token
                    ValidAudience = bearerTokenOption.Audience,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenOption.Key)),

                    // verify signature to avoid tampering
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true, // validate the expiration
                    // tolerance for the expiration date
                    ClockSkew = TimeSpan.Zero
                };

                cfg.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(JwtBearerEvents));

                        logger.LogError(context.Exception, message: "Authentication failed");

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var tokenValidatorService =
                            context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();

                        return tokenValidatorService.ValidateAsync(context);
                    },
                    OnMessageReceived = context => { return Task.CompletedTask; },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(JwtBearerEvents));

                        logger.LogError(context.Error, "OnChallenge error {Error}", context.ErrorDescription);

                        return Task.CompletedTask;
                    }
                };
            });
    }

    public static void AddCustomDbContext(this IServiceCollection services,
        IConfiguration configuration,
        Assembly startupAssembly)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var projectDir = ServerPath.GetProjectPath(startupAssembly);
        var defaultConnection = configuration.GetConnectionString(name: "DefaultConnection");

        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException(message: "defaultConnection is null");
        }

        var connectionString = defaultConnection.Replace(oldValue: "|DataDirectory|",
            Path.Combine(projectDir, path2: "wwwroot", path3: "app_data"), StringComparison.OrdinalIgnoreCase);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, serverDbContextOptionsBuilder =>
            {
                var minutes = (int)TimeSpan.FromMinutes(value: 3).TotalSeconds;
                serverDbContextOptionsBuilder.CommandTimeout(minutes);
                serverDbContextOptionsBuilder.EnableRetryOnFailure();
            });
        });
    }

    public static void AddCustomServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
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
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<BearerTokensOptions>()
            .Bind(configuration.GetSection(key: "BearerTokens"))
            .Validate(
                bearerTokens =>
                {
                    return bearerTokens.AccessTokenExpirationMinutes < bearerTokens.RefreshTokenExpirationMinutes;
                },
                failureMessage:
                "RefreshTokenExpirationMinutes is less than AccessTokenExpirationMinutes. Obtaining new tokens using the refresh token should happen only if the access token has expired.");

        services.AddOptions<ApiSettings>().Bind(configuration.GetSection(key: "ApiSettings"));
        services.AddOptions<AdminUserSeed>().Bind(configuration.GetSection(key: "AdminUserSeed"));
    }

    public static void UseCustomSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(setupAction =>
        {
            setupAction.SwaggerEndpoint(url: "/swagger/LibraryOpenAPISpecification/swagger.json", name: "Library API");

            //setupAction.RoutePrefix = ""; --> To be able to access it from this URL: https://localhost:5001/swagger/index.html

            setupAction.DefaultModelExpandDepth(depth: 2);
            setupAction.DefaultModelRendering(ModelRendering.Model);
            setupAction.DocExpansion(DocExpansion.None);
            setupAction.EnableDeepLinking();
            setupAction.DisplayOperationId();
        });
    }

    public static void AddCustomSwagger(this IServiceCollection services)
        => services.AddSwaggerGen(setupAction =>
        {
            setupAction.SwaggerDoc(name: "LibraryOpenAPISpecification", new OpenApiInfo
            {
                Title = "Library API",
                Version = "1",
                Description = "Through this API you can access the site's capabilities.",
                Contact = new OpenApiContact
                {
                    Email = "name@site.com",
                    Name = "DNT",
                    Url = new Uri(uriString: "http://www.dntips.ir")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri(uriString: "https://opensource.org/licenses/MIT")
                }
            });

            var xmlFiles = Directory
                .GetFiles(AppContext.BaseDirectory, searchPattern: "*.xml", SearchOption.TopDirectoryOnly)
                .ToList();

            xmlFiles.ForEach(xmlFile => setupAction.IncludeXmlComments(xmlFile));

            setupAction.AddSecurityDefinition(name: "Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    new List<string>()
                }
            });
        });
}