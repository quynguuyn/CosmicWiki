using MediatR;
using CosmicWiki.Pagination;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetAccounts;

public sealed record GetAdminAccountsQuery(string? Role, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AdminAccountResponse>>;

public sealed record AdminAccountResponse(
    Guid UserId,
    string Username,
    string Email,
    string Status,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Roles);
