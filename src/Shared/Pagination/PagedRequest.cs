namespace Shared.Pagination
{
    public record PagedRequest(int PageSize = 10, int PageNumber = 1);
}
