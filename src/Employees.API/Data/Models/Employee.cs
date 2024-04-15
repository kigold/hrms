using Shared.Data;

namespace Employees.API.Data.Models
{
    public class Employee : Entity<long>
    {
        public long Id { get; set; }
        public string StaffId { get; set; }
        public int CompanyId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public ICollection<Qualification> Qualifications { get; set; } = new List<Qualification>();
        public Company Company { get; set; }
    }
}
