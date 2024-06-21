using Auth.API.Apis;
using Auth.API.Data.Context;
using Auth.API.Data.Models;
using Auth.API.Messaging;
using Auth.API.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Shared.Messaging;
using Shared.Permissions;
using Shared.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static Shared.Messaging.PubMessageType;

namespace Auth.API
{
    public static class ConfigurationExtensions
    {
        private static bool IsDevelopment() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddTransient<DbContext, AuthDbContext>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IDbContextProvider<>), typeof(UnitOfWorkDbContextProvider<>));
            services.AddTransient<IRepository<User, long>, Repository<User, long, AuthDbContext>>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();           

            return services;
        }

        public static IServiceCollection AddMessageBus(this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<AuthMessageConsumer>().ExcludeFromConfigureEndpoints();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var configuration = context.GetRequiredService<IConfiguration>();
                    var host = configuration.GetConnectionString("RabbitMQConnection");
                    cfg.Host(host);
                    cfg.ConfigureEndpoints(context);

                    cfg.Publish<PublishMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                    });

                    cfg.ReceiveEndpoint("create-user", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<AuthMessageConsumer>(context);
                        e.Bind<PublishMessage>(x =>
                        {
                            x.ExchangeType = ExchangeType.Direct;
                            x.RoutingKey = EMPLOYEE_CREATE_USER;
                        });
                    });

                    cfg.ReceiveEndpoint("edit-user", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<AuthMessageConsumer>(context);
                        e.Bind<PublishMessage>(x =>
                        {
                            x.ExchangeType = ExchangeType.Direct;
                            x.RoutingKey = EMPLOYEE_UPDATE_USER;
                        });
                    });

                    cfg.ReceiveEndpoint("delete-user", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<AuthMessageConsumer>(context);
                        e.Bind<PublishMessage>(x =>
                        {
                            x.ExchangeType = ExchangeType.Direct;
                            x.RoutingKey = EMPLOYEE_DELETE_USER;
                        });
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();

            return services;
        }

        public static WebApplication MapCustomEnpoints(this WebApplication app)
        {
            app.MapControllers().RequireAuthorization();
                //.RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme });
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            app.MapDefaultControllerRoute();
            app.MapWeatherRequestEndpoints();
            app.MapRoleEndpoints();
            app.MapUserEndpoints();
            return app;
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
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
                var audience = "auth";
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = Claims.Name,
                    RoleClaimType = Claims.Role,
                    ValidateAudience = true,
                };

                options.Events = new JwtBearerEvents
                {
                    //OnMessageReceived = context =>
                    //{
                    //    Console.WriteLine($">>>>>>>>>>>>MessageReceived, {context.Token} {context.Principal} {context.Response}");
                    //    return Task.CompletedTask;
                    //},
                    //OnChallenge = context =>
                    //{
                    //    Console.WriteLine($">>>>>>>>>>>>Challenge, {context}");
                    //    return Task.CompletedTask;
                    //},
                    OnForbidden = context =>
                    {
                        var data = JsonSerializer.Serialize(context.HttpContext.User.Identity);
                        Console.WriteLine($">>>>>>>>>>>>Forbidden, {context}, {data}");
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
            .AddUserStore<UserStore<User, Role, AuthDbContext, long, IdentityUserClaim<long>, UserRole, IdentityUserLogin<long>, IdentityUserToken<long>, RoleClaim>>()
            .AddSignInManager()
            .AddUserManager<UserManager<User>>()
            .AddEntityFrameworkStores<AuthDbContext>()
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

            services.AddDbContext<AuthDbContext>(options =>
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
                           .UseDbContext<AuthDbContext>()
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
                        Scopes.OpenId,
                        Scopes.Roles,
                        "Permission",
                        "api"
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
                        //TODO Add Encryption
                    }


                    // Register scopes (permissions)
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, Scopes.OpenId, Scopes.Phone, "api");

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .DisableTransportSecurityRequirement();

                    // Register an event handler responsible for handling token requests.
                    //options.AddEventHandler<ProcessAuthenticationContext>(builder =>
                    //    builder.UseInlineHandler(context =>
                    //    {
                    //        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>Inline Event Handler");

                    //        return default;
                    //    }));
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