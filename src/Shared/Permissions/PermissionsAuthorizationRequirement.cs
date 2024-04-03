using Microsoft.AspNetCore.Authorization;

namespace Shared.Permissions
{
    public class PermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public Permission RequiredPermission { get; }
        public PermissionsAuthorizationRequirement(Permission requiredPermission)
        {
            RequiredPermission = requiredPermission; 
        }
    }
}