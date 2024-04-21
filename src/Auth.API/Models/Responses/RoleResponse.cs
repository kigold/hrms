namespace Auth.API.Models.Response
{
    public record RoleResponse(long Id, string Name);
    public record RoleWithUsersResponse(long Id, string Name, int UsersCount);
}
