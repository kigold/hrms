namespace Shared.Messaging
{
    public record EmployeeDTO(
            string Email,
            string FirstName,
            string LastName,
            string Phone
        );

    public record EmployeeStaffIdDTO(long StaffId, string Email);
}
