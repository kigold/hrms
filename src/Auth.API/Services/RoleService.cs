using Auth.API.Data.Models;
using Auth.API.Models.Request;
using Auth.API.Models.Requests;
using Auth.API.Models.Response;
using Auth.API.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Shared.Extensions;
using Shared.Pagination;
using Shared.Permissions;
using Shared.Repositories;
using Shared.ViewModels;
using System.Data;
using System.Security.Claims;

namespace Auth.API.Services
{
    public interface IRoleService
    {
        Task<ResultModel<PagedList<UserResponse>>> GetUsers(GetUsersRequest query);
        Task<ResultModel<PagedList<RoleResponse>>> GetRoles(PagedRequest query);
        Task<ResultModel<IEnumerable<PermissionResponse>>> GetAllPermissions();
        Task<ResultModel<IEnumerable<string>>> GetUserRoles(long userId);
        Task<ResultModel<List<PermissionResponse>>> GetRolePermissions(string roleName);

        Task<ResultModel<RoleResponse>> CreateRole(CreateRoleRequest request);
        Task<ResultModel<RoleResponse>> CloneRole(CloneRoleRequest request);
        Task<ResultModel> UpdateRolePermission(UpdateRolePermissionsRequest request);
        Task<ResultModel> AddPermissionsToRole(PermissionsRequest request);
        Task<ResultModel> RemovePermissionsFromRole(PermissionsRequest request);
        Task<ResultModel> AddUserToRoles(UpdateUserRolesRequest request);
        Task<ResultModel> RemoveUserFromRole(UpdateUserRolesRequest request);
        Task<ResultModel> AddPermissionsToUser(AddUserPermissionsRequest request);
        Task<ResultModel> RemovePermissionsFromUser(AddUserPermissionsRequest request);
        Task<ResultModel<bool>> DeleteRole(string roleName);
    }

    public class RoleService : IRoleService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User, long> _userRepo;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IRepository<User, long> userRepo,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
        }       

        public async Task<ResultModel<RoleResponse>> CreateRole(CreateRoleRequest request)
        {
            var validator = new CreateRoleRequestValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return new ResultModel<RoleResponse>() { ErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList() };
            }

            var role = await _roleManager.FindByNameAsync(request.Name);
            if (role != null)
                return new ResultModel<RoleResponse>("Role already exists");

            role = new Role
            {
                Name = request.Name
            };

            var createResult = await _roleManager.CreateAsync(role);

            if (!createResult.Succeeded)
                return new ResultModel<RoleResponse>(createResult.Errors.ToString());

            var permissions = request.PermissionIds.Select(x => (Permission)x);

            var addPermissionResult = await AddPermissionToRole(role, request.PermissionIds);
            if (addPermissionResult.HasError)
                return new ResultModel<RoleResponse>(addPermissionResult.ErrorMessages);

            return new ResultModel<RoleResponse>(new RoleResponse(role.Id, role.Name));
        }

        public async Task<ResultModel<RoleResponse>> CloneRole(CloneRoleRequest request)
        {
            var validator = new CloneRoleRequestValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return new ResultModel<RoleResponse>() { ErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList() };
            }

            var role = await _roleManager.FindByNameAsync(request.Name);
            if (role != null)
                return new ResultModel<RoleResponse>("Role already exists");

            role = new Role
            {
                Name = request.Name
            };

            //Get Permissions of Roles to Clone
            var permissions = new List<Permission>();
            foreach(var roleName in request.RolesToClone)
            {
                var roleToClone = await _roleManager.FindByNameAsync(roleName);
                if (roleToClone == null)
                    continue;

                permissions = permissions.Union(await QueryRolePermissions(roleName)).ToList();
            }
            if (permissions.Count < 1)
                return new ResultModel<RoleResponse>("No permission found in roles to clone");

            var createResult = await _roleManager.CreateAsync(role);

            if (!createResult.Succeeded)
                return new ResultModel<RoleResponse>(createResult.Errors.ToString());

            var addPermissionResult = await AddPermissionToRole(role, permissions.Select(x => (int)x).ToList());
            if (addPermissionResult.HasError)
                return new ResultModel<RoleResponse>(addPermissionResult.ErrorMessages);

            return new ResultModel<RoleResponse>(new RoleResponse(role.Id, role.Name));
        }

        public async Task<ResultModel<bool>> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return new ResultModel<bool>("Role does not exists");

            var roleUsers = await _userManager.GetUsersInRoleAsync(roleName);
            if (roleUsers.Any())
                return new ResultModel<bool>("Users are in role");

            var deleteResult = await _roleManager.DeleteAsync(role);

            if (!deleteResult.Succeeded)
                return new ResultModel<bool>(deleteResult.Errors.Select(x => x.Description).ToList());

            return new ResultModel<bool>(true, "Deleted role successfully");
        }

        public async Task<ResultModel> UpdateRolePermission(UpdateRolePermissionsRequest request)
        {
            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
                return new ResultModel<RoleResponse>("Role not found");

            var permissionTask = new[] { AddPermissionToRole(role, request.AddPermissionIds), RemovePermissionFromRole(role, request.RemovePermissionIds) };
            var result = await Task.WhenAll(permissionTask);

            var errorMessages = result.Where(x => x.HasError).ToList();
            if (errorMessages.Any())
                return new ResultModel(errorMessages.SelectMany(x => x.ErrorMessages).ToList());

            return new ResultModel();
        }

        private async Task<ResultModel> AddPermissionToRole(Role role, List<int> permissionIds)
        {
            var permissions = permissionIds.Where(x => Enum.IsDefined(typeof(Permission), x)).Select(x => (Permission)x);
            var errorMessages = new List<string>();

            foreach (var permission in permissions)
            {
                var addRoleResult = await _roleManager.AddClaimAsync(role, new Claim(nameof(Permission), permission.ToString()));
                if (!addRoleResult.Succeeded)
                {
                    errorMessages.Add($"Failed to add permission {permission.GetDescription()}: {addRoleResult.Errors.FirstOrDefault()?.Description}" ?? "Failed to add Permission");
                }
            }

            return errorMessages.Any() ? new ResultModel(errorMessages) : new ResultModel();
        }

        public async Task<ResultModel> AddPermissionsToRole(PermissionsRequest request)
        {
            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
                return new ResultModel<RoleResponse>("Role not found");

            var existingPermissions = (await _roleManager.GetClaimsAsync(role))
                                .Where(x => Enum.IsDefined(typeof(Permission), x.Value))
                                    .Select(x => (int)Enum.Parse<Permission>(x.Value));

            var addPermissionResult = await AddPermissionToRole(role, request.PermissionIds.Except(existingPermissions).ToList());
            if (addPermissionResult.HasError)
                return new ResultModel(addPermissionResult.ErrorMessages);

            return new ResultModel();
        }

        private async Task<ResultModel> RemovePermissionFromRole(Role role, List<int> permissionIds)
        {
            var permissions = permissionIds.Where(x => Enum.IsDefined(typeof(Permission), x)).Select(x => (Permission)x);
            var errorMessages = new List<string>();

            foreach (var permission in permissions)
            {
                var removePermissionResult = await _roleManager.RemoveClaimAsync(role, new Claim(nameof(Permission), permission.ToString()));
                if (!removePermissionResult.Succeeded)
                {
                    return new ResultModel(removePermissionResult.Errors.FirstOrDefault()?.Description??"");
                }
            }

            return errorMessages.Any() ? new ResultModel(errorMessages) : new ResultModel();
        }

        public async Task<ResultModel> RemovePermissionsFromRole(PermissionsRequest request)
        {
            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
                return new ResultModel("Role not found");

            return await RemovePermissionFromRole(role, request.PermissionIds);
        }

        public async Task<ResultModel> AddPermissionsToUser(AddUserPermissionsRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return new ResultModel("User not found");

            var permissions = request.PermissionIds.Where(x => Enum.IsDefined(typeof(Permission), x)).Select(x => (Permission)x);
            if (!permissions.Any())
                return new ResultModel("Invalid Permissions");

            var errorMessages = new List<string>();
            var result = await _userManager.AddClaimsAsync(user, permissions.Select(x => new Claim(nameof(Permission), x.ToString())));
            if (!result.Succeeded)
            {
                errorMessages.AddRange(result.Errors.Select(x => $"Failed to Add permissions: {x?.Description}" ?? "Failed to Add Permission"));
            }

            return errorMessages.Any() ? new ResultModel(errorMessages) : new ResultModel();
        }

        public async Task<ResultModel> RemovePermissionsFromUser(AddUserPermissionsRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return new ResultModel("User not found");

            var permissions = request.PermissionIds.Where(x => Enum.IsDefined(typeof(Permission), x)).Select(x => (Permission)x);
            if (!permissions.Any())
                return new ResultModel("Invalid Permissions");

            var errorMessages = new List<string>();
            var result = await _userManager.RemoveClaimsAsync(user, permissions.Select(x => new Claim(nameof(Permission), x.ToString())));
            if (!result.Succeeded)
            {
                errorMessages.AddRange(result.Errors.Select(x => $"Failed to Remove permissions: {x?.Description}" ?? "Failed to remove Permission"));
            }

            return errorMessages.Any() ? new ResultModel(errorMessages) : new ResultModel();
        }

        public async Task<ResultModel> AddUserToRoles(UpdateUserRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return new ResultModel("User not found");

            //Remove existing role if any
            var userRoles = await _userManager.GetRolesAsync(user);
            _ = await _userManager.RemoveFromRolesAsync(user, userRoles);

            IdentityResult? addRoleResult = null;
            try
            {
                addRoleResult = await _userManager.AddToRolesAsync(user, [request.RoleName]);
            }
            catch (Exception ex)
            {
                //Log
            }
            if (addRoleResult == null || !addRoleResult.Succeeded)
                return new ResultModel($"failed to add role, {string.Join("; ", addRoleResult?.Errors?.Select(x => x.Description) ?? [])}");

            return new ResultModel();
        }        

        public async Task<ResultModel> RemoveUserFromRole(UpdateUserRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return new ResultModel("User not found");

            var roleResult = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (!roleResult.Succeeded)
                return new ResultModel($"failed to remove role {request.RoleName} to user {user.FullName}. {roleResult.Errors.FirstOrDefault()?.Description}");

            return new ResultModel();
        }

        public async Task<ResultModel<IEnumerable<PermissionResponse>>> GetAllPermissions()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var permissions = (Enum.GetValues(typeof(Permission)) as Permission[])
                    .Select(x => new PermissionResponse((int)x, x.ToString(), x.GetDescription()));

            return new ResultModel<IEnumerable<PermissionResponse>>(permissions, "Success");
        }

        public async Task<ResultModel<List<PermissionResponse>>> GetRolePermissions(string roleName)
        {
            var permissions = await QueryRolePermissions(roleName);

            return new ResultModel<List<PermissionResponse>>(
                permissions.Select(x => new PermissionResponse((int)x, x.ToString(), x.GetDescription())).ToList());

        }

        private async Task<IEnumerable<Permission>> QueryRolePermissions(string roleName) //TODO Get ROles and Permissions with a single DB Query
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return Enumerable.Empty<Permission>();

            return (await _roleManager.GetClaimsAsync(role))
                .Where(x => Enum.IsDefined(typeof(Permission), x.Value))
                    .Select(x => Enum.Parse<Permission>(x.Value)).ToList();
        }

        public async Task<ResultModel<PagedList<UserResponse>>> GetUsers(GetUsersRequest query)
        {
            var users = _userRepo.Get();
            if (!string.IsNullOrEmpty(query.Query))
            {
                users = users.Where(x => x.Firstname == query.Query || x.Lastname == query.Query || x.Email == query.Query);
            }
            var result = PagedList<User>.ToPagedList(users.OrderBy(x => x.Id), query.PageNumber, query.PageSize);

            return new ResultModel<PagedList<UserResponse>>(new PagedList<UserResponse>(result.Items.Select(x => x.ToUserResponse()), result.TotalCount, query.PageNumber, query.PageSize));
        }

        public async Task<ResultModel<PagedList<RoleResponse>>> GetRoles(PagedRequest query)
        {
            var totalRoles = _roleManager.Roles.Count();
            var roles = new PagedList<RoleResponse>(
                _roleManager.Roles.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).OrderBy(x => x.Id).ToList().Select(x => new RoleResponse(x.Id, x.Name)),
                totalRoles,
                query.PageNumber,
                query.PageSize);
            return new ResultModel<PagedList<RoleResponse>>(roles);
        }

        public async Task<ResultModel<IEnumerable<string>>> GetUserRoles(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new ResultModel<IEnumerable<string>>("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            return new ResultModel<IEnumerable<string>>(roles);
        }        
    }

    public static class MapperExtensions
    {
        public static UserResponse ToUserResponse(this User source)
        {
            return new UserResponse(
                source.Id,
                source.Firstname,
                source.Lastname,
                source.Email??"",
                source.Avatar??""
                );
        }
    }
}