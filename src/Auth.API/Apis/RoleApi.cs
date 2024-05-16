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
            rolePermissionGroup.MapGet($"/Permissions/{{roleName}}", GetRolePermissions).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapGet("/Permissions/All", GetAllPermissions).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapGet($"/User/Roles/{{userId}}", GetUserRoles).RequireAuthorization(Permission.USER_READ.ToString());
            rolePermissionGroup.MapGet($"/User/Permissions/{{userId}}", GetUserPermissions).RequireAuthorization(Permission.USER_READ.ToString());
            rolePermissionGroup.MapGet($"/User/Permissions2/{{userId}}", GetUserPermissions2).RequireAuthorization(Permission.USER_READ.ToString());
            rolePermissionGroup.MapPost($"/", CreateRole).RequireAuthorization(Permission.ROLE_CREATE.ToString());
            rolePermissionGroup.MapPost($"/CloneRoles", CloneRoles).RequireAuthorization(Permission.ROLE_CREATE.ToString());
            rolePermissionGroup.MapPut($"/Permissions", UpdateRolePermission).RequireAuthorization(Permission.ROLE_UPDATE.ToString());
            rolePermissionGroup.MapPut($"/Permissions/Add", AddPermissionsToRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString());
            rolePermissionGroup.MapPut($"/Permissions/Remove", RemovePermissionsFromRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString()); ;
            rolePermissionGroup.MapPut($"/User/Add", AddUserToRoles).RequireAuthorization(Permission.USER_UPDATE.ToString()); ;
            rolePermissionGroup.MapPut($"/User/Remove", RemoveUserFromRole).RequireAuthorization(Permission.USER_UPDATE.ToString()); ;
            rolePermissionGroup.MapPut($"/User/Permissions/Add", AddPermissionsToUser).RequireAuthorization(Permission.USER_UPDATE.ToString());
            rolePermissionGroup.MapPut($"/User/Permissions/Remove", RemovePermissionsFromUser).RequireAuthorization(Permission.USER_UPDATE.ToString());
            rolePermissionGroup.MapPut($"/User/lock", LockoutUser).RequireAuthorization(Permission.USER_UPDATE.ToString());
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

        //GET /Permissions/{roleName} - GetRolePermissions
        public static async Task<Results<Ok<PermissionResponse[]>, ValidationProblem>> GetRolePermissions(IRoleService roleService, string roleName)
        {
            var result = await roleService.GetRolePermissions(roleName);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data.ToArray());
        }

        //GET /Permissions/All - GetAllPermissions
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

        //GET /User/Permissions/{userId} - GetUserPermissions
        public static async Task<Results<Ok<List<PermissionResponse>>, ValidationProblem>> GetUserPermissions(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserPermissions(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //GET /User/Permissions2/{userId} - GetUserPermissions2
        public static async Task<Results<Ok<List<PermissionResponse>>, ValidationProblem>> GetUserPermissions2(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserPermissions2(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
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

        //PUT /Permissions
        public static async Task<Results<NoContent, ValidationProblem>> UpdateRolePermission(IRoleService roleService, [FromBody] UpdateRolePermissionsRequest request)
        {
            var result = await roleService.UpdateRolePermission(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }


        //PUT /Permissions/Add
        public static async Task<Results<NoContent, ValidationProblem>> AddPermissionsToRole(IRoleService roleService, [FromBody] PermissionsRequest request)
        {
            var result = await roleService.AddPermissionsToRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /Permissions/Remove
        public static async Task<Results<NoContent, ValidationProblem>> RemovePermissionsFromRole(IRoleService roleService, [FromBody] PermissionsRequest request)
        {
            var result = await roleService.RemovePermissionsFromRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /User/Add
        public static async Task<Results<NoContent, ValidationProblem>> AddUserToRoles(IRoleService roleService, [FromBody] UpdateUserRolesRequest request)
        {
            var result = await roleService.AddUserToRoles(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /User/Remove
        public static async Task<Results<NoContent, ValidationProblem>> RemoveUserFromRole(IRoleService roleService, [FromBody] UpdateUserRolesRequest request)
        {
            var result = await roleService.RemoveUserFromRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /User/Lock
        public static async Task<Results<NoContent, ValidationProblem>> LockoutUser(IRoleService roleService, [FromBody] UpdateUserStatusRequest request)
        {
            var result = await roleService.LockoutUser(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /User/Permissions/Add
        public static async Task<Results<NoContent, ValidationProblem>> AddPermissionsToUser(IRoleService roleService, [FromBody] AddUserPermissionsRequest request)
        {
            var result = await roleService.AddPermissionsToUser(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /User/Permissions/Remove
        public static async Task<Results<NoContent, ValidationProblem>> RemovePermissionsFromUser(IRoleService roleService, [FromBody] AddUserPermissionsRequest request)
        {
            var result = await roleService.RemovePermissionsFromUser(request);
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