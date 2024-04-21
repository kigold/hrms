using Shared.Data;

namespace Employees.API.Data.Models
{
    public class Company : Entity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public Guid? LogoFileId { get; set; }
        public MediaFile LogoFile { get; set; }
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
