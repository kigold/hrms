using Auth.API.Models.Request;
using Auth.API.Models.Requests;
using Auth.API.Models.Response;
using Auth.API.Models.Responses;
using Auth.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Pagination;
using Shared.Permissions;

namespace Auth.API.Apis
{
    public static class RoleApi
    {
        private const string PATH_BASE = "role";
        private const string TAGS = "role api";

        public static WebApplication MapRoleEndpoints(this WebApplication app)
        {
            var rolePermissionGroup = app.MapGroup(PATH_BASE).WithTags(TAGS)
                .RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });
                //.RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme });
            rolePermissionGroup.MapGet("/User", GetUsers).RequireAuthorization(Permission.USER_READ.ToString());
            rolePermissionGroup.MapGet("/", GetRoles).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapGet("/Permissions", GetAllPermissions).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapGet($"/User/Roles/{{userId}}", GetUserRoles).RequireAuthorization(Permission.ROLE_READ.ToString()); ;
            rolePermissionGroup.MapGet($"/Permissions/{{roleName}}", GetRolePermissions).RequireAuthorization(Permission.ROLE_READ.ToString()); ;
            rolePermissionGroup.MapPost($"/", CreateRole).RequireAuthorization(Permission.ROLE_CREATE.ToString());
            rolePermissionGroup.MapPost($"/CloneRoles", CloneRoles).RequireAuthorization(Permission.ROLE_CREATE.ToString());
            rolePermissionGroup.MapPost($"/AddPermissionsToRole", AddPermissionsToRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString());
            rolePermissionGroup.MapPost($"/RemovePermissionsFromRole", RemovePermissionsFromRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString()); ;
            rolePermissionGroup.MapPost($"/AddUserToRoles", AddUserToRoles).RequireAuthorization(Permission.ROLE_UPDATE.ToString()); ;
            rolePermissionGroup.MapPost($"/RemoveUserFromRole", RemoveUserFromRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString()); ;
            rolePermissionGroup.MapPost($"/AddPermissionsToUser", AddPermissionsToUser).RequireAuthorization(Permission.ROLE_UPDATE.ToString()); ;
            rolePermissionGroup.MapDelete($"/{{roleName}}", DeleteRole).RequireAuthorization(Permission.ROLE_DELETE.ToString()); ;

            return app;
        }

        //GET /Users
        public static async Task<Results<Ok<PagedList<UserResponse>>, ValidationProblem>> GetUsers(IRoleService roleService, [AsParameters] GetUsersRequest query)
        {
            var result = await roleService.GetUsers(query);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //GET /
        public static async Task<Results<Ok<PagedList<RoleResponse>>, ValidationProblem>> GetRoles(IRoleService roleService, [AsParameters] PagedRequest query)
        {
            var result = await roleService.GetRoles(query);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //GET /Permissions - GetAllPermissions
        public static async Task<Results<Ok<PermissionResponse[]>, ValidationProblem>> GetAllPermissions(IRoleService roleService)
        {
            var result = await roleService.GetAllPermissions();
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data.ToArray());
        }

        //GET /User/Roles/{userId} - GetUserRoles
        public static async Task<Results<Ok<string[]>, ValidationProblem>> GetUserRoles(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserRoles(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data.ToArray());
        }

        //GET /Permissions{roleName} - GetRolePermissions
        public static async Task<Results<Ok<PermissionResponse[]>, ValidationProblem>> GetRolePermissions(IRoleService roleService, string roleName)
        {
            var result = await roleService.GetRolePermissions(roleName);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data.ToArray());
        }

        //POST /
        public static async Task<Results<Ok<RoleResponse>, ValidationProblem>> CreateRole(IRoleService roleService, [FromBody] CreateRoleRequest request)
        {
            var result = await roleService.CreateRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //POST /CloneRoles
        public static async Task<Results<Ok<RoleResponse>, ValidationProblem>> CloneRoles(IRoleService roleService, [FromBody] CloneRoleRequest request)
        {
            var result = await roleService.CloneRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //POST /AddPermissionsToRole
        public static async Task<Results<NoContent, ValidationProblem>> AddPermissionsToRole(IRoleService roleService, [FromBody] PermissionsRequest request)
        {
            var result = await roleService.AddPermissionsToRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //POST /RemovePermissionsFromRole
        public static async Task<Results<NoContent, ValidationProblem>> RemovePermissionsFromRole(IRoleService roleService, [FromBody] PermissionsRequest request)
        {
            var result = await roleService.RemovePermissionsFromRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //POST /AddUserToRoles
        public static async Task<Results<NoContent, ValidationProblem>> AddUserToRoles(IRoleService roleService, [FromBody] UpdateUserRolesRequest request)
        {
            var result = await roleService.AddUserToRoles(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //POST /RemoveUserFromRole
        public static async Task<Results<NoContent, ValidationProblem>> RemoveUserFromRole(IRoleService roleService, [FromBody] UpdateUserRolesRequest request)
        {
            var result = await roleService.RemoveUserFromRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //POST /AddPermissionsToUser
        public static async Task<Results<NoContent, ValidationProblem>> AddPermissionsToUser(IRoleService roleService, [FromBody] AddUserPermissionsRequest request)
        {
            var result = await roleService.AddPermissionsToUser(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //Delete /{roleName}
        public static async Task<Results<Ok<bool>, ValidationProblem>> DeleteRole(IRoleService roleService, string roleName)
        {
            var result = await roleService.DeleteRole(roleName);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }
    }
}