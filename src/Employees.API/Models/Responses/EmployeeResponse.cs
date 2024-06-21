namespace Employees.API.Models.Responses
{
    public record EmployeeResponse(
            long Id,
            string FirstName,
            string LastName,
            string Email,
            string Address,
            string Phone,
            string Country,
            string StaffId
        );

    public record EmployeeDetailResponse(
            long Id,
            string FirstName,
            string LastName,
            string Email,
            string Address,
            string Phone,
            string Country,
            string StaffId,
            IEnumerable<QualificationResponse> Qualifications
        );
}
