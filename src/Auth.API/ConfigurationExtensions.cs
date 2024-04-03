﻿using Auth.API.Apis;
using Auth.API.Data.Context;
using Auth.API.Data.Models;
using Auth.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using Shared.Permissions;
using Shared.Repositories;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.API
{
    public static class ConfigurationExtensions
    {
        private static bool IsDevelopment() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddTransient<DbContext, AppDbContext>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            //services.AddScoped(typeof(IDbContextProvider<>), typeof(UnitOfWorkDbContextProvider<>));
            services.AddTransient<IRepository<User>, Repository<User, AppDbContext>>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();

            return services;
        }

        public static WebApplication MapCustomEnpoints(this WebApplication app)
        {
            app.MapControllers().RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme });
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            app.MapDefaultControllerRoute();
            app.MapWeatherRequestEndpoints();
            app.MapRoleEndpoints();
            return app;
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var identityUrl = configuration["Authentication:IdentityUrl"];
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = Claims.Name,
                    RoleClaimType = Claims.Role,
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            //Authorization
            services.AddAuthorization(x => {
                x.AddPolicy("Role", p => p.RequireClaim("Role"));
                x.AddPermissionAuthorizationPolicy();
            });

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.User.RequireUniqueEmail = false;
            })
            .AddUserStore<UserStore<User, Role, AppDbContext, long, IdentityUserClaim<long>, UserRole, IdentityUserLogin<long>, IdentityUserToken<long>, RoleClaim>>()
            .AddSignInManager()
            .AddUserManager<UserManager<User>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24);
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.SignIn.RequireConfirmedAccount = false;
                // configure more options if necessary...
            });

            services.AddControllersWithViews();

            services.AddDbContext<AppDbContext>(options =>
            {
                // Configure Entity Framework Core to use Microsoft SQL Server.
                options.UseSqlServer(configuration.GetConnectionString("Default"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need to replace the default OpenIddict entities.
                options.UseOpenIddict<long>();
            });

            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<AppDbContext>()
                           .ReplaceDefaultEntities<long>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    options.RegisterScopes(Scopes.Email,
                        Scopes.Profile,
                        Scopes.Address,
                        Scopes.Phone,
                        Scopes.OfflineAccess,
                        Scopes.OpenId
                    );

                    // Enable the token endpoint.
                    options.SetTokenEndpointUris("/connect/token")
                           .SetAuthorizationEndpointUris("/connect/authorize")
                           .SetUserinfoEndpointUris("/connect/userinfo");

                    options.AllowPasswordFlow();
                    options.AcceptAnonymousClients();
                    options.AllowRefreshTokenFlow();

                    // Register the signing and encryption credentials.
                    if (IsDevelopment())
                    {
                        options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate()
                           .DisableAccessTokenEncryption();
                    }
                    else
                    {
                        //TODO Add Encrption
                    }


                    // Register scopes (permissions)
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, Scopes.OpenId, Scopes.Phone, "api");

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .DisableTransportSecurityRequirement();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            services.AddHostedService<SeedingWorker>(); //Seed Users
        }
    }
}

public class AuthorizationOptions : IAuthorizeData
{
    public string? Policy { get; set; }
    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }
}