using Employees.API.Data;
using Employees.API.Data.Models;
using Shared.Repositories;

namespace Employees.API
{
    public class SeedingWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedingWorker(IServiceProvider serviceProvider)
           => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
            await context.Database.EnsureCreatedAsync();

            await SeedUsers();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task SeedUsers()
        {
            var companies = new List<Company>
            {
                new Company{ Address = "Dallas", Country = "USA", Name = "IT Firm" },
                new Company{ Address = "Ontario", Country = "Canada", Name = "Biz Firm" },
                new Company{ Address = "Lagos", Country = "Nigeria", Name = "Gov Firm" }
            };
            using var scope = _serviceProvider.CreateScope();
            var companyRepo = scope.ServiceProvider.GetRequiredService<IRepository<Company, int>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            foreach (var company in companies)
            {
                if (companyRepo.Get(x => x.Name == company.Name).FirstOrDefault() is null)
                {
                    companyRepo.Insert(company);
                    var result = await unitOfWork.SaveChangesAsync();
                }
            }
        }
    }
}
