using FluentValidation;

namespace Auth.API.Models.Request
{
    public record CreateRoleRequest(string Name, List<int> PermissionIds);
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(role => role.Name).NotEmpty().Must(x => !x.Contains('|')).WithMessage("Role name contains invalid character |");
            RuleFor(role => role.PermissionIds).NotNull();
        }
    }

    public record CloneRoleRequest(string Name, List<string> RolesToClone);
    public class CloneRoleRequestValidator : AbstractValidator<CloneRoleRequest>
    {
        public CloneRoleRequestValidator()
        {
            RuleFor(role => role.Name).NotEmpty().Must(x => !x.Contains('|')).WithMessage("Role name contains invalid character |");
            RuleFor(role => role.RolesToClone).NotNull().NotEmpty().WithMessage("Role to clone is required");
        }
    }

    public record UpdateRolePermissionsRequest(string RoleName, List<int> AddPermissionIds, List<int> RemovePermissionIds);
    public record PermissionsRequest(string RoleName, List<int> PermissionIds);
    public record UpdateUserRolesRequest(long UserId, string RoleName);
    public record AddUserPermissionsRequest(long UserId, List<int> PermissionIds);
}
