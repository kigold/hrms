using Employees.API.Enums;
using Shared.Data;
using System;

namespace Employees.API.Data.Models
{
    public class Qualification : Entity<long>
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public QualificationType QualificationType { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public DateTime? DateReceived { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public Guid? MediaFileId { get; set; }
        public MediaFile MediaFile { get; set; }
        public Employee Employee { get; set; }
    }
}
