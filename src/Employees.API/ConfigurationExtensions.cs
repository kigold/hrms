﻿using Employees.API.Apis;
using Employees.API.Data;
using Employees.API.Data.Models;
using Employees.API.Messaging;
using Employees.API.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Shared.Data;
using Shared.FileStorage;
using Shared.Messaging;
using Shared.Permissions;
using Shared.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static Shared.Messaging.PubMessageType;

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

            return services;
        }

        public static IServiceCollection AddMessageBus(this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<FileProcessorConsumer>().ExcludeFromConfigureEndpoints();
                x.AddConsumer<EmployeeMessageConsumer>().ExcludeFromConfigureEndpoints();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var configuration = context.GetRequiredService<IConfiguration>();
                    var host = configuration.GetConnectionString("RabbitMQConnection");
                    cfg.Host(host);

                    cfg.Publish<PublishMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                    });

                    cfg.ReceiveEndpoint("set-staff-id", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<EmployeeMessageConsumer>(context);
                        e.Bind<PublishMessage>(x =>
                        {
                            x.ExchangeType = ExchangeType.Direct;
                        });
                    });

                    cfg.ReceiveEndpoint("delete-file", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<FileProcessorConsumer>(context);
                        e.Bind<PublishMessage>(x =>
                        {
                            x.ExchangeType = ExchangeType.Direct;
                            x.RoutingKey = FILE_PROCESS_DELETE_FILE;
                        });
                    });

                    cfg.ReceiveEndpoint("shrink-file", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Bind<PublishMessage>(x =>
                        {
                            x.ExchangeType = ExchangeType.Direct;
                            x.RoutingKey = FILE_PROCESS_SHRINK_FILE;
                        });
                    });

                    cfg.Send<PublishMessage>(x =>
                    {
                        // use customerType for the routing key
                        x.UseRoutingKeyFormatter(context => context.Message.MessageType.ToString());
                    });

                    cfg.ConfigureEndpoints(context);

                });
            });

            return services;
        }

        public static IServiceCollection AddFileProvider(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var fileSetting = builder.Configuration.GetSection(nameof(FileStorageSetting)).Get<FileStorageSetting>()!;
            services.Configure<FileStorageSetting>(builder.Configuration.GetSection(nameof(FileStorageSetting)));
            Directory.CreateDirectory(fileSetting.BaseDirectory);
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(fileSetting.BaseDirectory));

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService, EmployeeService>(); 
            services.AddHostedService<SeedingWorker>(); //Seed Companies

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
                    //OnMessageReceived = context =>
                    //{
                    //    Console.WriteLine($">>>>>>>>>>>>MessageReceived, {context.Token} {context.Principal} {context.Response}");
                    //    return Task.CompletedTask;
                    //},
                    //OnChallenge = context =>
                    //{
                    //    Console.WriteLine($">>>>>>>>>>>>Challenge, {context.AuthenticateFailure?.Message} {context.Error} {context.ErrorDescription}");
                    //    return Task.CompletedTask;
                    //},
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
