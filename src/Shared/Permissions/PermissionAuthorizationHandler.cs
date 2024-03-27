using Microsoft.AspNetCore.Authorization;

namespace Shared.Permissions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionsAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsAuthorizationRequirement requirement)
        {
            var currentUserPermissions = context.User.Claims
                .Where(x => x.Type.Equals(nameof(Permission))).Select(x => x.Value);

            if (currentUserPermissions.Contains(requirement.RequiredPermission.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}