using Auth.API.Models.Response;

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

    public record UserDetailResponse(
            UserResponse User,
            RoleResponse[] Roles,
            PermissionResponse[] Permissions
        );
}
