using FluentValidation;

namespace Employees.API.Models.Requests
{
    public record CreateEmployee(
            string FirstName,
            string LastName,
            string Email,
            string Phone,
            string Address,
            string Country,
            int CompanyId
        );

    public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployee>
    {
        public CreateEmployeeRequestValidator()
        {
            RuleFor(e => e.FirstName).NotEmpty().WithMessage("FirstName is required");
            RuleFor(e => e.LastName).NotEmpty().WithMessage("LastName is required");
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage("Email is required");
            RuleFor(e => e.Phone).NotEmpty().WithMessage("Phone is required");
            RuleFor(e => e.Country).NotEmpty().WithMessage("Country is required");
            RuleFor(e => e.CompanyId).NotEmpty().GreaterThan(0).WithMessage("CompanyId is required");
        }
    }

    public record UpdateEmployee(
        long EmployeeId,
        string FirstName,
        string LastName,
        string Phone,
        string Address,
        string Country
    );

    public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployee>
    {
        public UpdateEmployeeRequestValidator()
        {
            RuleFor(e => e.EmployeeId).NotEmpty().GreaterThan(0).WithMessage("EmployeeId is required");
            RuleFor(e => e.FirstName).NotEmpty().WithMessage("FirstName is required");
            RuleFor(e => e.LastName).NotEmpty().WithMessage("LastName is required");
            RuleFor(e => e.Phone).NotEmpty().WithMessage("Phone is required");
            RuleFor(e => e.Country).NotEmpty().WithMessage("Country is required");
        }
    }
}
