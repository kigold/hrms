using Employees.API.Data.Models;
using Microsoft.EntityFrameworkCore;

using Shared.Data;

namespace Employees.API.Data
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext()
        {
        }

        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<MediaFile> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(EmployeeDbContext).Assembly);

            //Company
            builder.Entity<Company>()
                .HasMany(c => c.Employees)
                .WithOne(e => e.Company);

            builder.Entity<Company>()
                .HasOne(e => e.LogoFile)
                .WithMany()
                .HasForeignKey(x => x.LogoFileId);

            //Employee
            builder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany(e => e.Employees);

            builder.Entity<Employee>()
                .HasMany(e => e.Qualifications)
                .WithOne(e => e.Employee);

            //Qualification
            builder.Entity<Qualification>()
                .HasOne(x => x.MediaFile)
                .WithMany()
                .HasForeignKey(x => x.MediaFileId);
        }

    }
}
