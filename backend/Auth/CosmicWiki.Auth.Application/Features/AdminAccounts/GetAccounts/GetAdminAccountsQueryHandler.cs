using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetAccounts;

public sealed class GetAdminAccountsQueryHandler(
    IAdminAccountRepository repository)
    : IRequestHandler<GetAdminAccountsQuery, PagedResult<AdminAccountResponse>>
{
    public async Task<PagedResult<AdminAccountResponse>> Handle(
        GetAdminAccountsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new ArgumentException("Page must be greater than zero.", nameof(request.Page));
        if (request.PageSize < 1 || request.PageSize > PaginationOptions.MaxPageSize)
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(request.PageSize));

        var (accounts, totalCount) = await repository.GetAccountsAsync(
            request.Role,
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            cancellationToken);
        var items = accounts.Select(x => new AdminAccountResponse(
            x.UserId, x.Username, x.Email, x.Status, x.CreatedAt, x.Roles))
            .ToArray();
        return new PagedResult<AdminAccountResponse>(
            items, request.Page, request.PageSize, totalCount);
    }
}
