namespace Auth.API.Models.Responses
{
    public record UserResponse(
            long Id,
            string FirstName,
            string LastName,
            string Email,
            string Avatar,
            bool IsActive
        );
}
