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
    public static class UserApi
    {
        private const string PATH_BASE = "user";
        private const string TAGS = "user api";

        public static WebApplication MapUserEndpoints(this WebApplication app)
        {
            var userGroup = app.MapGroup(PATH_BASE).WithTags(TAGS)
                .RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });
            userGroup.MapGet("/", GetUsers).RequireAuthorization(Permission.USER_READ.ToString());
            userGroup.MapGet("/{userId}", GetUserDetails).RequireAuthorization(Permission.USER_READ.ToString());
            userGroup.MapGet($"/Roles/{{userId}}", GetUserRoles).RequireAuthorization(Permission.USER_READ.ToString());
            userGroup.MapGet($"/Permissions/{{userId}}", GetUserPermissions).RequireAuthorization(Permission.USER_READ.ToString());
            userGroup.MapGet($"/Permissions2/{{userId}}", GetUserPermissions2).RequireAuthorization(Permission.USER_READ.ToString());
            userGroup.MapPut($"/Add", AddUserToRoles).RequireAuthorization(Permission.USER_UPDATE.ToString()); ;
            userGroup.MapPut($"/Remove", RemoveUserFromRole).RequireAuthorization(Permission.USER_UPDATE.ToString()); ;
            userGroup.MapPut($"/Permissions/Add", AddPermissionsToUser).RequireAuthorization(Permission.USER_UPDATE.ToString());
            userGroup.MapPut($"/Permissions/Remove", RemovePermissionsFromUser).RequireAuthorization(Permission.USER_UPDATE.ToString());
            userGroup.MapPut($"/Status", UpdateUserStatus).RequireAuthorization(Permission.USER_UPDATE.ToString());

            return app;
        }

        //GET /
        public static async Task<Results<Ok<PagedList<UserResponse>>, ValidationProblem>> GetUsers(IRoleService roleService, [AsParameters] GetUsersRequest query)
        {
            var result = await roleService.GetUsers(query);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //GET /{userId}
        public static async Task<Results<Ok<UserDetailResponse>, ValidationProblem>> GetUserDetails(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserDetails(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }
        
        //GET /Roles/{userId} - GetUserRoles
        public static async Task<Results<Ok<string[]>, ValidationProblem>> GetUserRoles(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserRoles(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data.ToArray());
        }

        //GET /Permissions/{userId} - GetUserPermissions
        public static async Task<Results<Ok<List<PermissionResponse>>, ValidationProblem>> GetUserPermissions(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserPermissions(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //GET /Permissions2/{userId} - GetUserPermissions2
        public static async Task<Results<Ok<List<PermissionResponse>>, ValidationProblem>> GetUserPermissions2(IRoleService roleService, long userId)
        {
            var result = await roleService.GetUserPermissions2(userId);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.Ok(result.Data);
        }

        //PUT /Add
        public static async Task<Results<NoContent, ValidationProblem>> AddUserToRoles(IRoleService roleService, [FromBody] UpdateUserRolesRequest request)
        {
            var result = await roleService.AddUserToRoles(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /Remove
        public static async Task<Results<NoContent, ValidationProblem>> RemoveUserFromRole(IRoleService roleService, [FromBody] UpdateUserRolesRequest request)
        {
            var result = await roleService.RemoveUserFromRole(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /Status
        public static async Task<Results<NoContent, ValidationProblem>> UpdateUserStatus(IRoleService roleService, [FromBody] UpdateUserStatusRequest request)
        {
            var result = await roleService.UpdateUserStatus(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /Permissions/Add
        public static async Task<Results<NoContent, ValidationProblem>> AddPermissionsToUser(IRoleService roleService, [FromBody] AddUserPermissionsRequest request)
        {
            var result = await roleService.AddPermissionsToUser(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }

        //PUT /Permissions/Remove
        public static async Task<Results<NoContent, ValidationProblem>> RemovePermissionsFromUser(IRoleService roleService, [FromBody] AddUserPermissionsRequest request)
        {
            var result = await roleService.RemovePermissionsFromUser(request);
            if (result.HasError)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });

            return TypedResults.NoContent();
        }
    }
}