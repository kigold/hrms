﻿using Auth.API.Data.Context;
using Auth.API.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.API
{
    public class SeedingWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedingWorker(IServiceProvider serviceProvider)
           => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            await SeedUsers();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task SeedUsers()
        {
            var users = new List<User>
            {
                new User { Firstname = "Admin", Lastname = "Mainman", Email = "admin@app.com", UserName = "admin@app.com", Avatar = "avatar-1.png" },
                new User { Firstname = "Tester", Lastname = "Handyman", Email = "tester@app.com", UserName = "tester@app.com", Avatar = "avatar-2.png" },
                new User { Firstname = "IT", Lastname = "Techbabe", Email = "it@app.com", UserName = "it@app.com", Avatar = "avatar-3.png" },
            };
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            foreach (var user in users)
            {
                if (await userManager.FindByNameAsync(user.UserName) is null)
                {
                    var hash = userManager.PasswordHasher.HashPassword(user, "P@ssw0rd");
                    user.PasswordHash = hash;
                    userManager.CreateAsync(user).GetAwaiter().GetResult();
                }
            }
        }
    }
}
