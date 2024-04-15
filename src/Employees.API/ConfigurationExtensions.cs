using Employees.API.Apis;
using Employees.API.Data;
using Employees.API.Data.Models;
using Employees.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Data;
using Shared.Permissions;
using Shared.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Employees.API
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<EmployeeDbContext>(options =>
            {
                // Configure Entity Framework Core to use Microsoft SQL Server.
                options.UseSqlServer(configuration.GetConnectionString("Default"));
            });

            services.AddTransient<DbContext, EmployeeDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IDbContextProvider<>), typeof(UnitOfWorkDbContextProvider<>));
            services.AddTransient<IRepository<Employee, long>, Repository<Employee, long, EmployeeDbContext>>();
            services.AddTransient<IRepository<Qualification, long>, Repository<Qualification, long, EmployeeDbContext>>();
            services.AddTransient<IRepository<Company, int>, Repository<Company, int, EmployeeDbContext>>();
            services.AddTransient<IRepository<MediaFile, Guid>, Repository<MediaFile, Guid, EmployeeDbContext>>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddHostedService<SeedingWorker>(); //Seed Companies

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService, EmployeeService>();

            return services;
        }

        public static WebApplication MapCustomEnpoints(this WebApplication app)
        {
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            app.MapEmployeeEndpoints();
            return app;
        }

        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            var identityUrl = configuration["Authentication:IdentityUrl"];
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var audience = "employee";
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = Claims.Name,
                    RoleClaimType = Claims.Role,
                    ValidateAudience = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine($">>>>>>>>>>>>MessageReceived, {context.Token} {context.Principal} {context.Response}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($">>>>>>>>>>>>Challenge, {context.AuthenticateFailure?.Message} {context.Error} {context.ErrorDescription}");
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        var data = JsonSerializer.Serialize(context.HttpContext.User.Identity);
                        Console.WriteLine($">>>>>>>>>>>>Forbidden, {context.HttpContext.User.Claims} {data}");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($">>>>>>>>>>>>Failed, {context}");
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            //Authorization
            services.AddAuthorization(x =>
            {
                x.AddPolicy("Role", p => p.RequireClaim("Role"));
                x.AddPermissionAuthorizationPolicy();
            });
        }
    }
}
