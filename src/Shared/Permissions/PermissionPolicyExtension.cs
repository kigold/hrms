using Microsoft.AspNetCore.Authorization;

namespace Shared.Permissions
{
    public static class PermissionPolicyExtension
    {
        public static void AddPermissionAuthorizationPolicy(this AuthorizationOptions authOptions)
        {
            foreach (Permission permission in Enum.GetValues(typeof(Permission)))
            {
                authOptions.AddPolicy(permission.ToString(), p => p.Requirements.Add(new PermissionsAuthorizationRequirement(permission)));
            }
        }
    }
}
