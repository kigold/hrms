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
            rolePermissionGroup.MapGet("/", GetRoles).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapGet($"/Permissions/{{roleName}}", GetRolePermissions).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapGet("/Permissions/All", GetAllPermissions).RequireAuthorization(Permission.ROLE_READ.ToString());
            rolePermissionGroup.MapPost($"/", CreateRole).RequireAuthorization(Permission.ROLE_CREATE.ToString());
            rolePermissionGroup.MapPost($"/CloneRoles", CloneRoles).RequireAuthorization(Permission.ROLE_CREATE.ToString());
            rolePermissionGroup.MapPut($"/Permissions", UpdateRolePermission).RequireAuthorization(Permission.ROLE_UPDATE.ToString());
            rolePermissionGroup.MapPut($"/Permissions/Add", AddPermissionsToRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString());
            rolePermissionGroup.MapPut($"/Permissions/Remove", RemovePermissionsFromRole).RequireAuthorization(Permission.ROLE_UPDATE.ToString());           
            rolePermissionGroup.MapDelete($"/{{roleName}}", DeleteRole).RequireAuthorization(Permission.ROLE_DELETE.ToString()); ;

            return app;
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