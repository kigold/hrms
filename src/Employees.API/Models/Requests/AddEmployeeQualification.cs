using Employees.API.Enums;
using FluentValidation;

namespace Employees.API.Models.Requests
{
    public record AddEmployeeQualification(
            long EmployeeId,
            QualificationType QualificationType,
            EducationLevel EducationLevel,
            string Title,
            string Description,
            DateTime? DateReceived,
            DateTime? ExpiryDate,
            IFormFile File
        );

    public class AddEmployeeQualificationValidator : AbstractValidator<AddEmployeeQualification>
    {
        public AddEmployeeQualificationValidator()
        {
            RuleFor(e => e.Title).NotEmpty().WithMessage("Title is required");
            RuleFor(e => e.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(e => e.QualificationType).IsInEnum().WithMessage("QualificationType is required");
            RuleFor(e => e.EducationLevel).IsInEnum().WithMessage("EducationLevel is required");
            RuleFor(e => e.EmployeeId).NotEmpty().GreaterThan(0).WithMessage("EmployeeId is required");
        }
    }
}
