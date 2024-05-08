using Shared.Pagination;

namespace Auth.API.Models.Requests
{
    public record GetUsersRequest(int PageSize = 10, int PageNumber = 1, string? Query = "") : PagedRequest(PageSize, PageNumber);
}