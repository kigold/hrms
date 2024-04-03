using FluentValidation;

namespace Auth.API.Models.Request
{
    public record CreateRoleRequest(string Name, List<int> PermissionIds);
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(role => role.Name).NotEmpty().Must(x => !x.Contains('|')).WithMessage("Role name contains invalid character |");
        }
    }

    public record PermissionsRequest(string RoleName, List<int> PermissionIds);
    public record UpdateUserRolesRequest(long UserId, string RoleName);
    public record AddUserPermissionsRequest(long UserId, List<int> PermissionIds);
}
