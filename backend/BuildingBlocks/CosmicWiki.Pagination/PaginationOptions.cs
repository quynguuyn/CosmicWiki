namespace CosmicWiki.Pagination;

public sealed record PaginationOptions(
    int Page = 1,
    int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int Skip =>
        (Page - 1) * PageSize;
}