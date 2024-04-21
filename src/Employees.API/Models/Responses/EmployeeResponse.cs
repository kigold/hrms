namespace Employees.API.Models.Responses
{
    public record EmployeeResponse(
            string FirstName,
            string LastName,
            string Email,
            string Address,
            string Phone,
            string Country,
            IEnumerable<QualificationResponse> Qualifications
        );
}
